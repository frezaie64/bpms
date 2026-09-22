using BPMS.Shared.Abstractions;

namespace BPMS.Modules.FormGenerator.Entities;

public class GeneratedForm : BaseEntity, ITenantEntity, IAuditableEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string CreatorUserId { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? ExpireDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public ICollection<FormField> Fields { get; set; } = [];
    public ICollection<FormSubmission> Submissions { get; set; } = [];
}