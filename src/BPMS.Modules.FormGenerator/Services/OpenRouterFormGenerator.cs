using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BPMS.Modules.FormGenerator.Models;
using Microsoft.Extensions.Options;

namespace BPMS.Modules.FormGenerator.Services;

public sealed class OpenRouterFormGenerator : IFormGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<OpenRouterOptions> _options;
    private readonly AiGenerationState _generationState;

    public OpenRouterFormGenerator(HttpClient httpClient, IOptionsMonitor<OpenRouterOptions> options, AiGenerationState generationState)
    {
        _httpClient = httpClient;
        _options = options;
        _generationState = generationState;
    }

    public async Task<GenerateFormResponse> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("prompt is required");
        }

        var apiKey = FirstNonEmpty(_options.CurrentValue.ApiKey, Environment.GetEnvironmentVariable("OPENROUTER_API_KEY"));
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OPENROUTER_API_KEY is not set");
        }

        var model = FirstNonEmpty(_options.CurrentValue.Model, Environment.GetEnvironmentVariable("OPENROUTER_MODEL"), "openai/gpt-4o-mini");
        var endpoint = FirstNonEmpty(_options.CurrentValue.Endpoint, Environment.GetEnvironmentVariable("OPENROUTER_ENDPOINT"), "https://openrouter.ai/api/v1/chat/completions");

        var requestBody = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = LlmFormSystemPrompt() },
                new { role = "user", content = prompt }
            },
            temperature = 0.2
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.TryAddWithoutValidation("HTTP-Referer",
            FirstNonEmpty(_options.CurrentValue.Referer, Environment.GetEnvironmentVariable("OPENROUTER_REFERER"), "http://localhost:5000"));
        request.Headers.TryAddWithoutValidation("X-Title",
            FirstNonEmpty(_options.CurrentValue.Title, Environment.GetEnvironmentVariable("OPENROUTER_TITLE"), "BPMS FormGenerator"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        _generationState.SetRawResponse(responseText);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"OpenRouter HTTP status {(int)response.StatusCode}: {responseText}");
        }

        using var document = JsonDocument.Parse(responseText);
        var choices = document.RootElement.GetProperty("choices");
        if (choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("OpenRouter response has no choices");
        }

        var message = choices[0].GetProperty("message");
        if (!message.TryGetProperty("content", out var contentElement) || contentElement.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException("OpenRouter response has no message content");
        }

        var payload = JsonSerializer.Deserialize<LlmFormPayload>(StripMarkdownFence(contentElement.GetString() ?? string.Empty), JsonOptions)
                      ?? throw new InvalidOperationException("failed to parse model output json");

        var form = new GenerateFormResponse
        {
            Name = FallbackText(payload.Name, InferFormName(prompt.Trim().ToLowerInvariant())),
            Description = FallbackText(payload.Description, "AI generated form"),
            Prompt = prompt,
            Fields = NormalizeFields(payload.Fields)
        };

        if (form.Fields.Count == 0)
        {
            throw new InvalidOperationException("model returned empty fields");
        }

        return form;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }

    private static string StripMarkdownFence(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            trimmed = trimmed.TrimStart('`').TrimStart();
            if (trimmed.StartsWith("json", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed[4..].TrimStart();
            }
            trimmed = trimmed.TrimEnd('`').Trim();
        }
        return trimmed;
    }

    private static string InferFormName(string prompt)
    {
        if (prompt.Contains("feedback"))
            return "Customer Feedback Form";
        if (prompt.Contains("job") || prompt.Contains("hiring") || prompt.Contains("candidate"))
            return "Job Application Form";
        if (prompt.Contains("support") || prompt.Contains("issue"))
            return "Support Request Form";
        return "Custom Request Form";
    }

    private static string FallbackText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static List<FormFieldDto> NormalizeFields(IEnumerable<LlmFormField> fields)
    {
        var normalized = new List<FormFieldDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in fields)
        {
            var key = FormValidator.SanitizeKey(field.Key);
            if (string.IsNullOrWhiteSpace(key) || !seen.Add(key))
                continue;

            normalized.Add(new FormFieldDto
            {
                Key = key,
                Label = FallbackText(field.Label?.Trim() ?? string.Empty, LabelFromKey(key)),
                Type = FormValidator.NormalizeType(field.Type),
                Required = field.Required,
                Placeholder = field.Placeholder?.Trim() ?? string.Empty,
                Options = field.Options?
                    .Select(item => item.Trim())
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .ToList() ?? [],
                OrderIndex = normalized.Count + 1
            });
        }

        return normalized;
    }

    private static string LabelFromKey(string key)
    {
        var parts = key.Split('_', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i][1..];
        }
        return string.Join(" ", parts);
    }

    private static string LlmFormSystemPrompt()
    {
        return """
You create web form definitions.
Return JSON only with this exact structure:
{
  "name": "string",
  "description": "string",
  "fields": [
    {
      "key": "snake_case_key",
      "label": "string",
      "type": "text|email|number|textarea|select|date|jalali_date|checkbox",
      "required": true,
      "placeholder": "string",
      "options": ["string"]
    }
  ]
}
Rules:
- Keep 3 to 10 fields.
- keys must be unique snake_case.
- options required only for select, otherwise empty array.
- output valid JSON, no markdown.
""";
    }

    private sealed class LlmFormPayload
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<LlmFormField> Fields { get; set; } = [];
    }

    private sealed class LlmFormField
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = "text";
        public bool Required { get; set; }
        public string Placeholder { get; set; } = string.Empty;
        public List<string> Options { get; set; } = [];
    }
}