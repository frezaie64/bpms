using BPMS.Modules.Identity.Entities;
using BPMS.Modules.Tenancy.Entities;
using BPMS.Shared.Persistence;
using BPMS.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BPMS.Modules.Tenancy;

public class TenantAccessService : ITenantAccessService
{
    private readonly BpmsDbContext _db;

    public TenantAccessService(BpmsDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        // TenantMember has no tenant query filter — safe to query directly.
        return await _db.Set<TenantMember>()
            .AnyAsync(tm => tm.TenantId == tenantId && tm.UserId == userId, ct);
    }

    public async Task<bool> IsGlobalAdminAsync(Guid userId, CancellationToken ct = default)
    {
        // Global roles (e.g. the seeded platform admin) live with TenantId == "".
        // IgnoreQueryFilters bypasses the Role tenant filter so this check is
        // deterministic regardless of the current tenant context.
        return await _db.Set<UserRole>()
            .IgnoreQueryFilters()
            .Where(ur => ur.UserId == userId)
            .AnyAsync(ur => ur.Role.TenantId == "", ct);
    }
}
