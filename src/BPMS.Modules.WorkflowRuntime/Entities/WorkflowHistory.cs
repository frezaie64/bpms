using BPMS.Shared.Abstractions;

namespace BPMS.Modules.WorkflowRuntime.Entities;

public class WorkflowHistory : BaseEntity
{
    public Guid WorkflowInstanceId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? NodeId { get; set; }
    public string? NodeName { get; set; }
    public string? Data { get; set; }
    public DateTime Timestamp { get; set; }
    public string? PerformedBy { get; set; }

    public WorkflowInstance? WorkflowInstance { get; set; }
}