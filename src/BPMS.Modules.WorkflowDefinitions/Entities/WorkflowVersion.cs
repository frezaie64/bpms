using BPMS.Shared.Abstractions;

namespace BPMS.Modules.WorkflowDefinitions.Entities;

public class WorkflowVersion : BaseEntity
{
    public Guid WorkflowId { get; set; }
    public int VersionNumber { get; set; }
    public string JsonDefinition { get; set; } = "{}";
    public WorkflowStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }

    public Workflow Workflow { get; set; } = null!;
}