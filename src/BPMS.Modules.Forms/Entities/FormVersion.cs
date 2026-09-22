using BPMS.Shared.Abstractions;

namespace BPMS.Modules.Forms.Entities;

public class FormVersion : BaseEntity
{
    public Guid FormId { get; set; }
    public int VersionNumber { get; set; }
    public string JsonDefinition { get; set; } = "{}";
    public FormStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }

    public Form Form { get; set; } = null!;
}