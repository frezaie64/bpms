using BPMS.Shared.Abstractions;

namespace BPMS.Modules.Tenancy.Entities;

public enum TenantStatus
{
    Active = 0,
    Suspended = 1,
    Disabled = 2
}

public class Tenant : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public Guid? PlanId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public List<TenantMember> Members { get; set; } = [];
}