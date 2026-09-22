using BPMS.Modules.FormGenerator.Models;
using BPMS.Modules.FormGenerator.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BPMS.Modules.FormGenerator;

public static class FormGeneratorModuleRegistration
{
    public static IServiceCollection AddFormGeneratorModule(this IServiceCollection services)
    {
        services.AddScoped<AiGenerationState>();
        services.AddScoped<AdminAiSettingsStore>();
        services.AddScoped<IFormGeneratorModule, FormGeneratorModule>();
        services.AddScoped<GeneratedFormService>();

        services.AddHttpClient<OpenRouterFormGenerator>((sp, client) =>
        {
            var timeoutSeconds = int.TryParse(Environment.GetEnvironmentVariable("OPENROUTER_TIMEOUT_SECONDS"), out var parsedTimeout)
                ? parsedTimeout
                : 25;
            client.Timeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds));
        });

        services.AddScoped<IFormGenerator>(sp => sp.GetRequiredService<OpenRouterFormGenerator>());

        return services;
    }
}