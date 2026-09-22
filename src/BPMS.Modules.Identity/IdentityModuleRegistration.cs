using BPMS.Modules.Identity.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BPMS.Modules.Identity;

public static class IdentityModuleRegistration
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<IIdentityModule, IdentityModule>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPasswordPolicy, PasswordPolicy>();
        return services;
    }
}