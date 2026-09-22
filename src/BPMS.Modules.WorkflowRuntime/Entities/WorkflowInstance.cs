using BPMS.Shared.Abstractions;

namespace BPMS.Modules.WorkflowRuntime.Entities;

public class WorkflowInstance : BaseEntity, ITenantEntity, IAuditableEntity
{
    public string TenantId { get; set; } = string.Empty;
    public Guid WorkflowId { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public int WorkflowVersion { get; set; }
    public WorkflowInstanceStatus Status { get; set; } = WorkflowInstanceStatus.Running;
    public string StartedBy { get; set; } = string.Empty;
    public DateTime StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string ActiveNodeIds { get; set; } = "[]";
    public string CompletedNodeIds { get; set; } = "[]";
    public string Variables { get; set; } = "{}";
    public string JsonDefinition { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public ICollection<WorkflowTask> Tasks { get; set; } = [];
    public ICollection<WorkflowHistory> History { get; set; } = [];
}