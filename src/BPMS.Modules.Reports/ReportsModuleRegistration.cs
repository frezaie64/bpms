using BPMS.Modules.Reports.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BPMS.Modules.Reports;

public static class ReportsModuleRegistration
{
    public static IServiceCollection AddReportsModule(this IServiceCollection services)
    {
        services.AddScoped<IReportsModule, ReportsModule>();
        services.AddScoped<ExcelExportService>();
        services.AddScoped<PdfExportService>();
        return services;
    }
}