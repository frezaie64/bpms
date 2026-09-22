using BPMS.Shared.Abstractions;

namespace BPMS.Modules.WorkflowDefinitions.Entities;

public class WorkflowSubscription : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = string.Empty;
    public Guid WorkflowId { get; set; }
    public string UserId { get; set; } = string.Empty;
}