using Microsoft.Extensions.DependencyInjection;

namespace BPMS.Modules.WorkflowDefinitions;

public static class WorkflowDefinitionsModuleRegistration
{
    public static IServiceCollection AddWorkflowDefinitionsModule(this IServiceCollection services)
    {
        services.AddScoped<IWorkflowDefinitionsModule, WorkflowDefinitionsModule>();
        return services;
    }
}