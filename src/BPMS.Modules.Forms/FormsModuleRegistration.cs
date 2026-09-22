using Microsoft.Extensions.DependencyInjection;

namespace BPMS.Modules.Forms;

public static class FormsModuleRegistration
{
    public static IServiceCollection AddFormsModule(this IServiceCollection services)
    {
        services.AddScoped<IFormsModule, FormsModule>();
        return services;
    }
}