using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BPMS.Modules.FormGenerator.Models;

public class SubmitFormRequest
{
    [JsonPropertyName("submitted_by")]
    public string SubmittedBy { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    [Required]
    public Dictionary<string, object?> Data { get; set; } = [];
}

public class SubmitFormResponse
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public string SubmittedBy { get; set; } = string.Empty;
    public Dictionary<string, object?> Data { get; set; } = [];
    public DateTime SubmittedAt { get; set; }
}