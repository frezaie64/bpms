using BPMS.Modules.FormGenerator.Models;

namespace BPMS.Modules.FormGenerator.Services;

public static class DeterministicFormGenerator
{
    public static GenerateFormResponse Generate(string prompt)
    {
        var lowerPrompt = prompt.Trim().ToLowerInvariant();
        var form = new GenerateFormResponse
        {
            Name = InferFormName(lowerPrompt),
            Description = "AI generated form",
            Prompt = prompt,
            Fields = []
        };

        form.Fields.AddRange([
            new FormFieldDto { Key = "full_name", Label = "Full Name", Type = "text", Required = true, Placeholder = "Enter full name", OrderIndex = 1 },
            new FormFieldDto { Key = "email", Label = "Email", Type = "email", Required = true, Placeholder = "Enter email", OrderIndex = 2 }
        ]);

        if (lowerPrompt.Contains("feedback"))
        {
            form.Fields.Add(new FormFieldDto { Key = "rating", Label = "Rating", Type = "select", Required = true, Options = ["1", "2", "3", "4", "5"], OrderIndex = 3 });
            form.Fields.Add(new FormFieldDto { Key = "comments", Label = "Comments", Type = "textarea", Required = false, Placeholder = "Tell us your feedback", OrderIndex = 4 });
        }
        else if (lowerPrompt.Contains("job") || lowerPrompt.Contains("hiring") || lowerPrompt.Contains("candidate"))
        {
            form.Fields.Add(new FormFieldDto { Key = "phone", Label = "Phone Number", Type = "text", Required = true, Placeholder = "Enter phone number", OrderIndex = 3 });
            form.Fields.Add(new FormFieldDto { Key = "experience_years", Label = "Years of Experience", Type = "number", Required = true, Placeholder = "e.g. 5", OrderIndex = 4 });
            form.Fields.Add(new FormFieldDto { Key = "resume_url", Label = "Resume URL", Type = "text", Required = true, Placeholder = "Paste resume link", OrderIndex = 5 });
        }
        else if (lowerPrompt.Contains("support") || lowerPrompt.Contains("issue"))
        {
            form.Fields.Add(new FormFieldDto { Key = "issue_type", Label = "Issue Type", Type = "select", Required = true, Options = ["billing", "technical", "account", "other"], OrderIndex = 3 });
            form.Fields.Add(new FormFieldDto { Key = "description", Label = "Issue Description", Type = "textarea", Required = true, Placeholder = "Describe your issue", OrderIndex = 4 });
            form.Fields.Add(new FormFieldDto { Key = "priority", Label = "Priority", Type = "select", Required = true, Options = ["low", "medium", "high"], OrderIndex = 5 });
        }
        else
        {
            form.Fields.Add(new FormFieldDto { Key = "details", Label = "Details", Type = "textarea", Required = true, Placeholder = "Add details", OrderIndex = 3 });
        }

        if (lowerPrompt.Contains("jalali") || lowerPrompt.Contains("jalaali") || lowerPrompt.Contains("shamsi") || lowerPrompt.Contains("persian date"))
        {
            form.Fields.Add(new FormFieldDto { Key = "jalali_date", Label = "Jalali Date", Type = "jalali_date", Required = true, Placeholder = "1405/01/15", OrderIndex = form.Fields.Count + 1 });
        }

        return form;
    }

    public static string ToJson(GenerateFormResponse form)
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            name = form.Name,
            description = form.Description,
            fields = form.Fields.Select(field => new
            {
                key = field.Key,
                label = field.Label,
                type = field.Type,
                required = field.Required,
                placeholder = field.Placeholder,
                options = field.Options
            })
        }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
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
}