using BPMS.Shared.Abstractions;

namespace BPMS.Modules.FormGenerator.Entities;

public class FormField : BaseEntity
{
    public Guid FormId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public bool Required { get; set; }
    public string Placeholder { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public int OrderIndex { get; set; }

    public GeneratedForm Form { get; set; } = null!;
}