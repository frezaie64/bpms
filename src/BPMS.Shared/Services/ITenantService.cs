namespace BPMS.Shared.Services;

public interface ITenantService
{
    string TenantId { get; }
    string? UserId { get; }
    void SetTenant(string tenantId);
    void SetUser(string? userId);
}