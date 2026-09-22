using BPMS.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BPMS.Modules.Tenancy;

public static class TenancyModuleRegistration
{
    public static IServiceCollection AddTenancyModule(this IServiceCollection services)
    {
        services.AddScoped<ITenancyModule, TenancyModule>();
        services.AddScoped<ITenantAccessService, TenantAccessService>();
        return services;
    }
}