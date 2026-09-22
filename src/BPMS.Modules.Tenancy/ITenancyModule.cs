using BPMS.Modules.Tenancy.Entities;

namespace BPMS.Modules.Tenancy;

public interface ITenancyModule
{
    // Tenant
    Task<Tenant> CreateTenantAsync(string name, string slug, Guid ownerUserId, CancellationToken ct = default);
    Task<List<Tenant>> GetTenantsAsync(CancellationToken ct = default);
    Task<Tenant?> GetTenantByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Tenant>> GetUserTenantsAsync(Guid userId, CancellationToken ct = default);

    // Plans
    Task<List<Plan>> GetPlansAsync(CancellationToken ct = default);

    // Subscriptions & Payments
    Task<Subscription> CreateSubscriptionAsync(Guid userId, Guid planId, CancellationToken ct = default);
    Task<List<Subscription>> GetUserSubscriptionsAsync(Guid userId, CancellationToken ct = default);
    Task<Subscription?> GetActiveSubscriptionForUserAsync(Guid userId, CancellationToken ct = default);
    Task<Payment> SimulatePaymentAsync(Guid subscriptionId, Guid userId, CancellationToken ct = default);

    // Members
    Task<TenantMember> AddMemberAsync(Guid tenantId, Guid userId, TenantMemberRole role, CancellationToken ct = default);
    Task<List<TenantMember>> GetMembersAsync(Guid tenantId, CancellationToken ct = default);

    // Invitations
    Task<Invitation> SendInvitationAsync(Guid tenantId, string email, TenantMemberRole role, Guid invitedByUserId, CancellationToken ct = default);
    Task<TenantMember> AcceptInvitationAsync(string token, Guid userId, CancellationToken ct = default);
    Task<List<Invitation>> GetTenantInvitationsAsync(Guid tenantId, CancellationToken ct = default);

    // Settings & Lookups
    Task<Models.TenantSettingResponse> GetSettingsAsync(CancellationToken ct = default);
    Task<Models.TenantSettingResponse> UpdateSettingsAsync(Models.UpdateTenantSettingRequest request, CancellationToken ct = default);
    Task<Models.LookupResponse> CreateLookupAsync(Models.CreateLookupRequest request, CancellationToken ct = default);
    Task<Models.LookupResponse> UpdateLookupAsync(Guid id, Models.UpdateLookupRequest request, CancellationToken ct = default);
    Task DeleteLookupAsync(Guid id, CancellationToken ct = default);
    Task<List<Models.LookupResponse>> GetLookupsAsync(string? group, CancellationToken ct = default);
}
