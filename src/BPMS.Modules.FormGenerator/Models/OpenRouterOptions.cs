namespace BPMS.Modules.FormGenerator.Models;

public sealed class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Referer { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}