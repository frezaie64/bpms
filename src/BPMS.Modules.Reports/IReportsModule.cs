using BPMS.Modules.Reports.Models;

namespace BPMS.Modules.Reports;

public interface IReportsModule
{
    Task<DashboardReportResponse> GetDashboardReportAsync(CancellationToken ct = default);
    Task<List<WorkflowReportItem>> GetWorkflowReportAsync(WorkflowReportFilter filter, CancellationToken ct = default);
    Task<List<TaskReportItem>> GetTaskReportAsync(TaskReportFilter filter, CancellationToken ct = default);
    Task<FormReportResponse> GetFormReportAsync(CancellationToken ct = default);
    Task<UserActivityReportResponse> GetUserActivityReportAsync(UserActivityFilter filter, CancellationToken ct = default);
    Task<SearchResponse> SearchAsync(string query, CancellationToken ct = default);
}