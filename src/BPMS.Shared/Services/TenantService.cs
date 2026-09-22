namespace BPMS.Shared.Services;

public class TenantService : ITenantService
{
    public string TenantId { get; private set; } = string.Empty;
    public string? UserId { get; private set; }

    public void SetTenant(string tenantId) => TenantId = tenantId;
    public void SetUser(string? userId) => UserId = userId;
}