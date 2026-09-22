using System.Security.Cryptography;
using BPMS.Modules.Identity.Entities;
using BPMS.Modules.Tenancy.Entities;
using BPMS.Modules.Tenancy.Models;
using BPMS.Shared.Abstractions;
using BPMS.Shared.Persistence;
using BPMS.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BPMS.Modules.Tenancy;

public class TenancyModule : ITenancyModule
{
    private readonly BpmsDbContext _db;
    private readonly ITenantService _tenant;

    public TenancyModule(BpmsDbContext db, ITenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    // ── Tenant ──────────────────────────────────────────────

    public async Task<Tenant> CreateTenantAsync(string name, string slug, Guid ownerUserId, CancellationToken ct = default)
    {
        // Validate user has an active subscription with completed payment
        var activeSub = await GetActiveSubscriptionForUserAsync(ownerUserId, ct);
        if (activeSub is null)
            throw new InvalidOperationException("An active subscription with completed payment is required to create a workspace.");

        var tenant = new Tenant
        {
            Name = name,
            Slug = slug,
            PlanId = activeSub.PlanId,
            SubscriptionId = activeSub.Id,
            Status = TenantStatus.Active
        };

        _db.Add(tenant);
        await _db.SaveChangesAsync(ct);

        // Switch the request's tenant context to the new tenant so every
        // ITenantEntity saved from here on (members, settings, roles) gets
        // the correct TenantId instead of being overwritten with "".
        _tenant.SetTenant(tenant.Id.ToString());

        // Add owner as TenantMember
        _db.Add(new TenantMember
        {
            TenantId = tenant.Id,
            UserId = ownerUserId,
            Role = TenantMemberRole.Owner
        });

        // Update user
        var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Id == ownerUserId, ct);
        if (user != null)
        {
            user.TenantId = tenant.Id.ToString();
            user.Status = UserStatus.Active;
        }

        // Auto-create TenantSetting with defaults
        _db.Set<TenantSetting>().Add(new TenantSetting
        {
            TenantId = tenant.Id.ToString(),
            TenantName = name,
            DefaultFileSizeLimit = 10,
            DefaultPageSize = 20,
            Language = "en",
            TimeZone = "UTC",
            DateFormat = "yyyy-MM-dd"
        });

        // Provision a tenant-scoped "Owner" role with all permissions and
        // assign it to the owner so their JWT carries permission claims.
        await ProvisionOwnerRoleAsync(ownerUserId, ct);

        await _db.SaveChangesAsync(ct);
        return tenant;
    }

    private async Task ProvisionOwnerRoleAsync(Guid ownerUserId, CancellationToken ct)
    {
        var ownerRole = new Role
        {
            Name = "Owner",
            TenantId = _tenant.TenantId,
            CreatedBy = ownerUserId.ToString()
        };
        _db.Add(ownerRole);

        var permissions = await _db.Set<Permission>().ToListAsync(ct);
        foreach (var permission in permissions)
        {
            _db.Add(new RolePermission
            {
                RoleId = ownerRole.Id,
                PermissionId = permission.Id
            });
        }

        _db.Add(new UserRole { UserId = ownerUserId, RoleId = ownerRole.Id });
    }

    public async Task<List<Tenant>> GetTenantsAsync(CancellationToken ct = default)
    {
        return await _db.Set<Tenant>().ToListAsync(ct);
    }

    public async Task<Tenant?> GetTenantByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Set<Tenant>().FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<List<Tenant>> GetUserTenantsAsync(Guid userId, CancellationToken ct = default)
    {
        var tenantIds = await _db.Set<TenantMember>()
            .Where(tm => tm.UserId == userId)
            .Select(tm => tm.TenantId)
            .ToListAsync(ct);

        return await _db.Set<Tenant>()
            .Where(t => tenantIds.Contains(t.Id))
            .ToListAsync(ct);
    }

    // ── Plans ───────────────────────────────────────────────

    public async Task<List<Plan>> GetPlansAsync(CancellationToken ct = default)
    {
        return await _db.Set<Plan>().Where(p => p.IsActive).ToListAsync(ct);
    }

    // ── Subscriptions & Payments ────────────────────────────

    public async Task<Subscription> CreateSubscriptionAsync(Guid userId, Guid planId, CancellationToken ct = default)
    {
        var plan = await _db.Set<Plan>().FirstOrDefaultAsync(p => p.Id == planId, ct);
        if (plan is null)
            throw new InvalidOperationException("Plan not found.");

        var subscription = new Subscription
        {
            UserId = userId,
            PlanId = planId,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1)
        };

        _db.Add(subscription);
        await _db.SaveChangesAsync(ct);

        // Create associated payment record
        _db.Add(new Payment
        {
            SubscriptionId = subscription.Id,
            UserId = userId,
            Amount = plan.Price,
            Status = PaymentStatus.Pending
        });

        await _db.SaveChangesAsync(ct);
        return subscription;
    }

    public async Task<List<Subscription>> GetUserSubscriptionsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Set<Subscription>()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(ct);
    }

    public async Task<Subscription?> GetActiveSubscriptionForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Set<Subscription>()
            .Where(s => s.UserId == userId
                && s.Status == SubscriptionStatus.Active
                && _db.Set<Payment>().Any(p => p.SubscriptionId == s.Id && p.Status == PaymentStatus.Completed))
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Payment> SimulatePaymentAsync(Guid subscriptionId, Guid userId, CancellationToken ct = default)
    {
        var payment = await _db.Set<Payment>()
            .FirstOrDefaultAsync(p => p.SubscriptionId == subscriptionId && p.UserId == userId, ct);

        if (payment is null)
            throw new InvalidOperationException("Payment not found.");

        if (payment.Status == PaymentStatus.Completed)
            throw new InvalidOperationException("Payment already completed.");

        payment.Status = PaymentStatus.Completed;
        payment.TransactionId = $"SIM-{Guid.NewGuid():N}";
        await _db.SaveChangesAsync(ct);
        return payment;
    }

    // ── Members ─────────────────────────────────────────────

    public async Task<TenantMember> AddMemberAsync(Guid tenantId, Guid userId, TenantMemberRole role, CancellationToken ct = default)
    {
        var existing = await _db.Set<TenantMember>()
            .FirstOrDefaultAsync(tm => tm.TenantId == tenantId && tm.UserId == userId, ct);
        if (existing != null)
            throw new InvalidOperationException("User is already a member of this tenant.");

        // Enforce plan limits
        var tenant = await _db.Set<Tenant>().FirstOrDefaultAsync(t => t.Id == tenantId, ct);
        if (tenant?.PlanId is not null)
        {
            var plan = await _db.Set<Plan>().FirstOrDefaultAsync(p => p.Id == tenant.PlanId, ct);
            if (plan is not null)
            {
                var memberCount = await _db.Set<TenantMember>().CountAsync(m => m.TenantId == tenantId, ct);
                if (memberCount >= plan.MaxUsers)
                    throw new InvalidOperationException($"Plan limit reached: maximum {plan.MaxUsers} users allowed.");
            }
        }

        var member = new TenantMember
        {
            TenantId = tenantId,
            UserId = userId,
            Role = role
        };

        _db.Add(member);

        var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user != null)
        {
            user.TenantId = tenantId.ToString();
            user.Status = UserStatus.Active;
        }

        await _db.SaveChangesAsync(ct);
        return member;
    }

    public async Task<List<TenantMember>> GetMembersAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _db.Set<TenantMember>()
            .Where(tm => tm.TenantId == tenantId)
            .ToListAsync(ct);
    }

    // ── Invitations ─────────────────────────────────────────

    public async Task<Invitation> SendInvitationAsync(Guid tenantId, string email, TenantMemberRole role, Guid invitedByUserId, CancellationToken ct = default)
    {
        // Validate caller is Owner or Admin of the tenant
        var callerMember = await _db.Set<TenantMember>()
            .FirstOrDefaultAsync(tm => tm.TenantId == tenantId && tm.UserId == invitedByUserId, ct);

        if (callerMember is null || (callerMember.Role != TenantMemberRole.Owner && callerMember.Role != TenantMemberRole.Admin))
            throw new InvalidOperationException("Only Owner or Admin can send invitations.");

        // Check plan limits
        var tenant = await _db.Set<Tenant>().FirstOrDefaultAsync(t => t.Id == tenantId, ct);
        if (tenant?.PlanId is not null)
        {
            var plan = await _db.Set<Plan>().FirstOrDefaultAsync(p => p.Id == tenant.PlanId, ct);
            if (plan is not null)
            {
                var memberCount = await _db.Set<TenantMember>().CountAsync(m => m.TenantId == tenantId, ct);
                if (memberCount >= plan.MaxUsers)
                    throw new InvalidOperationException($"Plan limit reached: maximum {plan.MaxUsers} users allowed.");
            }
        }

        // Check for existing pending invitation
        var existingInvite = await _db.Set<Invitation>()
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Email == email && i.Status == InvitationStatus.Pending, ct);
        if (existingInvite is not null)
            throw new InvalidOperationException("A pending invitation already exists for this email.");

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var invitation = new Invitation
        {
            TenantId = tenantId,
            Email = email,
            Role = role,
            InvitedByUserId = invitedByUserId,
            Token = token,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _db.Add(invitation);
        await _db.SaveChangesAsync(ct);

        // TODO: Send invitation email with token link
        // Example link: /invitations/accept?token={token}

        return invitation;
    }

    public async Task<TenantMember> AcceptInvitationAsync(string token, Guid userId, CancellationToken ct = default)
    {
        var invitation = await _db.Set<Invitation>()
            .FirstOrDefaultAsync(i => i.Token == token && i.Status == InvitationStatus.Pending, ct);

        if (invitation is null)
            throw new InvalidOperationException("Invalid or already used invitation token.");

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await _db.SaveChangesAsync(ct);
            throw new InvalidOperationException("Invitation has expired.");
        }

        // Check if user already a member
        var existingMember = await _db.Set<TenantMember>()
            .FirstOrDefaultAsync(tm => tm.TenantId == invitation.TenantId && tm.UserId == userId, ct);
        if (existingMember is not null)
            throw new InvalidOperationException("You are already a member of this workspace.");

        // Add user as TenantMember
        var member = new TenantMember
        {
            TenantId = invitation.TenantId,
            UserId = userId,
            Role = invitation.Role
        };
        _db.Add(member);

        // Update user
        var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user != null)
        {
            user.TenantId = invitation.TenantId.ToString();
            user.Status = UserStatus.Active;
        }

        // Mark invitation as accepted
        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return member;
    }

    public async Task<List<Invitation>> GetTenantInvitationsAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _db.Set<Invitation>()
            .Where(i => i.TenantId == tenantId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    // ── Settings ────────────────────────────────────────────

    public async Task<Models.TenantSettingResponse> GetSettingsAsync(CancellationToken ct = default)
    {
        var settings = await _db.Set<TenantSetting>()
            .FirstOrDefaultAsync(ct);

        if (settings is null)
        {
            settings = new TenantSetting
            {
                TenantId = _tenant.TenantId,
                DefaultFileSizeLimit = 10,
                DefaultPageSize = 20,
                Language = "en",
                TimeZone = "UTC",
                DateFormat = "yyyy-MM-dd"
            };
            _db.Set<TenantSetting>().Add(settings);
            await _db.SaveChangesAsync(ct);
        }

        return MapSettingResponse(settings);
    }

    public async Task<Models.TenantSettingResponse> UpdateSettingsAsync(Models.UpdateTenantSettingRequest request, CancellationToken ct)
    {
        var settings = await _db.Set<TenantSetting>()
            .FirstOrDefaultAsync(ct);

        if (settings is null)
        {
            settings = new TenantSetting { TenantId = _tenant.TenantId };
            _db.Set<TenantSetting>().Add(settings);
        }

        settings.TenantName = request.TenantName ?? settings.TenantName;
        settings.Logo = request.Logo ?? settings.Logo;
        settings.TimeZone = request.TimeZone ?? settings.TimeZone;
        settings.DateFormat = request.DateFormat ?? settings.DateFormat;
        settings.Language = request.Language ?? settings.Language;
        settings.DefaultFileSizeLimit = request.DefaultFileSizeLimit ?? settings.DefaultFileSizeLimit;
        settings.DefaultPageSize = request.DefaultPageSize ?? settings.DefaultPageSize;
        settings.SmtpHost = request.SmtpHost ?? settings.SmtpHost;
        settings.SmtpPort = request.SmtpPort ?? settings.SmtpPort;
        settings.SmtpUsername = request.SmtpUsername ?? settings.SmtpUsername;
        settings.SmtpPassword = request.SmtpPassword ?? settings.SmtpPassword;
        settings.SenderName = request.SenderName ?? settings.SenderName;
        settings.SenderEmail = request.SenderEmail ?? settings.SenderEmail;
        settings.SmsProviderName = request.SmsProviderName ?? settings.SmsProviderName;
        settings.SmsApiUrl = request.SmsApiUrl ?? settings.SmsApiUrl;
        settings.SmsApiKey = request.SmsApiKey ?? settings.SmsApiKey;
        settings.SmsSenderId = request.SmsSenderId ?? settings.SmsSenderId;

        await _db.SaveChangesAsync(ct);
        return MapSettingResponse(settings);
    }

    // ── Lookups ─────────────────────────────────────────────

    public async Task<Models.LookupResponse> CreateLookupAsync(Models.CreateLookupRequest request, CancellationToken ct)
    {
        var lookup = new LookupValue
        {
            TenantId = _tenant.TenantId,
            Group = request.Group,
            Name = request.Name,
            Value = request.Value
        };
        _db.Set<LookupValue>().Add(lookup);
        await _db.SaveChangesAsync(ct);
        return MapLookupResponse(lookup);
    }

    public async Task<Models.LookupResponse> UpdateLookupAsync(Guid id, Models.UpdateLookupRequest request, CancellationToken ct)
    {
        var lookup = await _db.Set<LookupValue>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Lookup with ID {id} not found.");
        lookup.Group = request.Group;
        lookup.Name = request.Name;
        lookup.Value = request.Value;
        lookup.IsActive = request.IsActive ?? lookup.IsActive;
        await _db.SaveChangesAsync(ct);
        return MapLookupResponse(lookup);
    }

    public async Task DeleteLookupAsync(Guid id, CancellationToken ct)
    {
        var lookup = await _db.Set<LookupValue>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Lookup with ID {id} not found.");
        lookup.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<Models.LookupResponse>> GetLookupsAsync(string? group, CancellationToken ct)
    {
        var query = _db.Set<LookupValue>().AsQueryable();
        if (!string.IsNullOrEmpty(group))
            query = query.Where(l => l.Group == group);
        var lookups = await query.OrderBy(l => l.Group).ThenBy(l => l.Name).ToListAsync(ct);
        return lookups.Select(MapLookupResponse).ToList();
    }

    // ── Mappers ─────────────────────────────────────────────

    private static Models.TenantSettingResponse MapSettingResponse(TenantSetting s)
    {
        return new Models.TenantSettingResponse(
            s.Id, s.TenantName, s.Logo, s.TimeZone, s.DateFormat, s.Language,
            s.DefaultFileSizeLimit, s.DefaultPageSize,
            s.SmtpHost, s.SmtpPort, s.SmtpUsername, s.SenderName, s.SenderEmail,
            s.SmsProviderName, s.SmsApiUrl, s.SmsSenderId,
            s.CreatedAt, s.UpdatedAt
        );
    }

    private static Models.LookupResponse MapLookupResponse(LookupValue l)
    {
        return new Models.LookupResponse(l.Id, l.Group, l.Name, l.Value, l.IsActive, l.CreatedAt, l.UpdatedAt);
    }
}
