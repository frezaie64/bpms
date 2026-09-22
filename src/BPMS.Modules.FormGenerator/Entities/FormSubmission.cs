using BPMS.Shared.Abstractions;

namespace BPMS.Modules.FormGenerator.Entities;

public class FormSubmission : BaseEntity
{
    public Guid FormId { get; set; }
    public string SubmittedBy { get; set; } = string.Empty;
    public Dictionary<string, object?> Data { get; set; } = [];
    public DateTime SubmittedAt { get; set; }

    public GeneratedForm Form { get; set; } = null!;
}