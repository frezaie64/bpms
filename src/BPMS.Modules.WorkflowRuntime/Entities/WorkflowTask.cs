using BPMS.Shared.Abstractions;

namespace BPMS.Modules.WorkflowRuntime.Entities;

public class WorkflowTask : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = string.Empty;
    public Guid WorkflowInstanceId { get; set; }
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public WorkflowTaskType TaskType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AssignedToUserId { get; set; }
    public string? AssignedToRole { get; set; }
    public WorkflowTaskStatus Status { get; set; } = WorkflowTaskStatus.Pending;
    public string? CompletedBy { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Comments { get; set; }
    public string? FormData { get; set; }

    public WorkflowInstance? WorkflowInstance { get; set; }
}