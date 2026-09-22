using Microsoft.Extensions.DependencyInjection;

namespace BPMS.Modules.Notifications;

public static class NotificationsModuleRegistration
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddScoped<INotificationsModule, NotificationsModule>();
        return services;
    }
}