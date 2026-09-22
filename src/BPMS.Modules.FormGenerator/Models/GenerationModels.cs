using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BPMS.Modules.FormGenerator.Models;

public class GenerateFormRequest
{
    [JsonPropertyName("prompt")]
    [Required]
    public string Prompt { get; set; } = string.Empty;
}

public class GenerateFormResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string CreatorUserId { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? ExpireDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<FormFieldDto> Fields { get; set; } = [];
    public bool UsedFallback { get; set; }
    public string? Error { get; set; }
}

public class FormFieldDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public bool Required { get; set; }
    public string Placeholder { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public int OrderIndex { get; set; }
}

public class AcceptFormRequest
{
    [Required]
    public GenerateFormResponse Form { get; set; } = new();
}

public class FormDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string CreatorUserId { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? ExpireDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<FormFieldDto> Fields { get; set; } = [];
}