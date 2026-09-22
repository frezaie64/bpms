using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BPMS.Modules.FormGenerator.Models;

namespace BPMS.Modules.FormGenerator.Services;

public static partial class FormValidator
{
    public static GenerateFormResponse SanitizeAndValidateForm(GenerateFormResponse form)
    {
        ArgumentNullException.ThrowIfNull(form);

        form.Name = FallbackText(form.Name.Trim(), "Custom Request Form");
        form.Description = FallbackText(form.Description.Trim(), "AI generated form");
        form.Prompt = form.Prompt?.Trim() ?? string.Empty;

        if (form.Fields.Count == 0)
        {
            throw new ArgumentException("form must contain at least one field");
        }

        if (form.StartDate.HasValue)
            form.StartDate = form.StartDate.Value.ToUniversalTime();

        if (form.ExpireDate.HasValue)
            form.ExpireDate = form.ExpireDate.Value.ToUniversalTime();

        if (form.StartDate.HasValue && form.ExpireDate.HasValue && form.StartDate.Value > form.ExpireDate.Value)
        {
            throw new ArgumentException("start_date cannot be later than expire_date");
        }

        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalizedFields = new List<FormFieldDto>(form.Fields.Count);

        for (var i = 0; i < form.Fields.Count; i++)
        {
            var field = form.Fields[i];
            var key = SanitizeKey(field.Key);
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"field key is invalid at position {i + 1}");

            if (!seenKeys.Add(key))
                throw new ArgumentException($"duplicate field key: {key}");

            normalizedFields.Add(new FormFieldDto
            {
                Key = key,
                Label = FallbackText(field.Label?.Trim() ?? string.Empty, LabelFromKey(key)),
                Type = NormalizeType(field.Type),
                Required = field.Required,
                Placeholder = field.Placeholder?.Trim() ?? string.Empty,
                Options = NormalizeOptions(field.Options),
                OrderIndex = i + 1
            });
        }

        form.Fields = normalizedFields;
        return form;
    }

    public static void ValidateFormAvailability(DateTime? startDate, DateTime? expireDate)
    {
        var now = DateTime.UtcNow;
        if (startDate.HasValue && now < startDate.Value.ToUniversalTime())
            throw new InvalidOperationException("form is not available yet");

        if (expireDate.HasValue && now > expireDate.Value.ToUniversalTime())
            throw new InvalidOperationException("form has expired");
    }

    public static void ValidateSubmission(IEnumerable<FormFieldDto> fields, IReadOnlyDictionary<string, object?> data)
    {
        foreach (var field in fields)
        {
            data.TryGetValue(field.Key, out var value);
            if (field.Required && (!data.ContainsKey(field.Key) || value is null || string.IsNullOrWhiteSpace(value.ToString())))
                throw new ArgumentException($"required field missing: {field.Key}");

            if (data.ContainsKey(field.Key) && value is not null && field.Type == "jalali_date" && !IsValidJalaliDate(value.ToString() ?? string.Empty))
                throw new ArgumentException($"invalid jalali_date format for field: {field.Key}");
        }
    }

    public static string NormalizeType(string type)
    {
        var normalized = type?.Trim().ToLowerInvariant() ?? string.Empty;
        return normalized switch
        {
            "text" or "email" or "number" or "textarea" or "select" or "date" or "checkbox" or "jalali_date" => normalized,
            "jalali" or "jalaali" or "persian_date" or "shamsi_date" => "jalali_date",
            _ => "text"
        };
    }

    public static string SanitizeKey(string input)
    {
        var builder = new StringBuilder();
        var lastWasUnderscore = false;

        foreach (var ch in input.Trim().ToLowerInvariant())
        {
            if (ch == ' ')
            {
                if (!lastWasUnderscore && builder.Length > 0)
                {
                    builder.Append('_');
                    lastWasUnderscore = true;
                }
                continue;
            }

            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_')
            {
                builder.Append(ch);
                lastWasUnderscore = ch == '_';
            }
        }

        return builder.ToString().Trim('_').Replace("__", "_", StringComparison.Ordinal);
    }

    public static bool IsValidJalaliDate(string value)
    {
        return JalaliDatePattern().IsMatch(NormalizePersianDigits(value).Trim());
    }

    private static List<string> NormalizeOptions(IEnumerable<string>? options)
    {
        return options?
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList() ?? [];
    }

    private static string FallbackText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static string LabelFromKey(string key)
    {
        var parts = key.Split('_', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length == 0) continue;
            parts[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parts[i]);
        }
        return string.Join(" ", parts);
    }

    private static string NormalizePersianDigits(string input)
    {
        return input
            .Replace("۰", "0", StringComparison.Ordinal)
            .Replace("۱", "1", StringComparison.Ordinal)
            .Replace("۲", "2", StringComparison.Ordinal)
            .Replace("۳", "3", StringComparison.Ordinal)
            .Replace("۴", "4", StringComparison.Ordinal)
            .Replace("۵", "5", StringComparison.Ordinal)
            .Replace("۶", "6", StringComparison.Ordinal)
            .Replace("۷", "7", StringComparison.Ordinal)
            .Replace("۸", "8", StringComparison.Ordinal)
            .Replace("۹", "9", StringComparison.Ordinal)
            .Replace("٠", "0", StringComparison.Ordinal)
            .Replace("١", "1", StringComparison.Ordinal)
            .Replace("٢", "2", StringComparison.Ordinal)
            .Replace("٣", "3", StringComparison.Ordinal)
            .Replace("٤", "4", StringComparison.Ordinal)
            .Replace("٥", "5", StringComparison.Ordinal)
            .Replace("٦", "6", StringComparison.Ordinal)
            .Replace("٧", "7", StringComparison.Ordinal)
            .Replace("٨", "8", StringComparison.Ordinal)
            .Replace("٩", "9", StringComparison.Ordinal);
    }

    [GeneratedRegex(@"^(13|14)\d{2}[-\/](0[1-9]|1[0-2])[-\/](0[1-9]|[12]\d|3[01])$")]
    private static partial Regex JalaliDatePattern();
}