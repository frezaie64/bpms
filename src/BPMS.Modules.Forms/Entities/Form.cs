using BPMS.Shared.Abstractions;

namespace BPMS.Modules.Forms.Entities;

public class Form : BaseEntity, ITenantEntity, IAuditableEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public FormStatus Status { get; set; } = FormStatus.Draft;
    public int CurrentVersion { get; set; } = 1;
    public string JsonDefinition { get; set; } = "{}";
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public FormCategory? Category { get; set; }
    public ICollection<FormVersion> Versions { get; set; } = [];
}