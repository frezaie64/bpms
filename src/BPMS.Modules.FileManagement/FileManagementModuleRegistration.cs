using BPMS.Modules.FileManagement.Models;
using BPMS.Modules.FileManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;

namespace BPMS.Modules.FileManagement;

public static class FileManagementModuleRegistration
{
    public static IServiceCollection AddFileManagementModule(this IServiceCollection services)
    {
        services.AddScoped<IFileManagementModule, FileManagementModule>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        services.AddSingleton<IMinioClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
            return new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .Build();
        });

        return services;
    }
}