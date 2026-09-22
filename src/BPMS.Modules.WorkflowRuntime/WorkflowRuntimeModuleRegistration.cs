using BPMS.Modules.WorkflowRuntime.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BPMS.Modules.WorkflowRuntime;

public static class WorkflowRuntimeModuleRegistration
{
    public static IServiceCollection AddWorkflowRuntimeModule(this IServiceCollection services)
    {
        services.AddScoped<IWorkflowRuntimeModule, WorkflowRuntimeModule>();
        services.AddScoped<WorkflowEngine>();
        services.AddScoped<ISystemTaskService, SystemTaskService>();
        return services;
    }
}