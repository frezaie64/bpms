using BPMS.Shared.Abstractions;

namespace BPMS.Modules.Notifications.Entities;

public class AuditLog : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string Entity { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
}