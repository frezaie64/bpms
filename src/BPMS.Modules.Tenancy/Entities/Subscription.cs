using BPMS.Shared.Abstractions;

namespace BPMS.Modules.Tenancy.Entities;

public enum SubscriptionStatus
{
    Active = 0,
    Expired = 1,
    Cancelled = 2
}

public class Subscription : BaseEntity, IAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}