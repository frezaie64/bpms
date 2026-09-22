using BPMS.Shared.Abstractions;

namespace BPMS.Modules.WorkflowDefinitions.Entities;

public class Workflow : BaseEntity, ITenantEntity, IAuditableEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
    public int CurrentVersion { get; set; } = 1;
    public string JsonDefinition { get; set; } = "{}";
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public ICollection<WorkflowVersion> Versions { get; set; } = [];
}