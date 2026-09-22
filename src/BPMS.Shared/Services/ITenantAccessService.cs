namespace BPMS.Shared.Services;

/// <summary>
/// Validates whether a user may operate within a tenant context.
/// Implemented by the Tenancy module (which owns tenant membership data)
/// and used by the TenantMiddleware to prevent cross-tenant spoofing
/// via the X-Tenant-Id header.
/// </summary>
public interface ITenantAccessService
{
    /// <summary>True if the user is a member of the given tenant.</summary>
    Task<bool> IsMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    /// <summary>True if the user holds a global (TenantId == "") role, e.g. the seeded platform admin.</summary>
    Task<bool> IsGlobalAdminAsync(Guid userId, CancellationToken ct = default);
}
