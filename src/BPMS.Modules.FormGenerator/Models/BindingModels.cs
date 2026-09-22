using System.Text.Json.Serialization;

namespace BPMS.Modules.FormGenerator.Models;

public class BindFormRequest
{
    [JsonPropertyName("task_id")]
    public string TaskId { get; set; } = string.Empty;

    [JsonPropertyName("form_id")]
    public Guid FormId { get; set; }
}

public class BindFormResponse
{
    public string Status { get; set; } = "bound";
}