namespace BPMS.Modules.FormGenerator.Entities;

public class TaskFormBinding
{
    public string TaskId { get; set; } = string.Empty;
    public Guid FormId { get; set; }
    public DateTime CreatedAt { get; set; }

    public GeneratedForm Form { get; set; } = null!;
}