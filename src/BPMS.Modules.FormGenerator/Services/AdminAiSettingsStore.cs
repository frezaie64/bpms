using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace BPMS.Modules.FormGenerator.Services;

public sealed class AdminAiSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly IWebHostEnvironment _environment;

    public AdminAiSettingsStore(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<Models.OpenRouterOptions> LoadAsync(CancellationToken cancellationToken = default)
    {
        var path = GetSettingsPath();
        if (!File.Exists(path))
            return new Models.OpenRouterOptions();

        await using var stream = File.OpenRead(path);
        var settings = await JsonSerializer.DeserializeAsync<AdminSettingsFile>(stream, JsonOptions, cancellationToken);
        return settings?.OpenRouter ?? new Models.OpenRouterOptions();
    }

    public async Task SaveAsync(Models.OpenRouterOptions options, CancellationToken cancellationToken = default)
    {
        var current = await LoadAsync(cancellationToken);
        current.ApiKey = options.ApiKey ?? string.Empty;
        current.Model = options.Model ?? string.Empty;
        current.Endpoint = options.Endpoint ?? string.Empty;
        current.Referer = options.Referer ?? string.Empty;
        current.Title = options.Title ?? string.Empty;

        var settings = new AdminSettingsFile { OpenRouter = current };

        var path = GetSettingsPath();
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
    }

    private string GetSettingsPath()
    {
        return Path.Combine(_environment.ContentRootPath, "appsettings.admin.json");
    }

    private sealed class AdminSettingsFile
    {
        public Models.OpenRouterOptions OpenRouter { get; set; } = new();
    }
}