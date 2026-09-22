using BPMS.Modules.Reports.Models;
using BPMS.Modules.Reports.Services;
using BPMS.Shared.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.Reports.Endpoints;

public static class ReportsEndpoints
{
    public static void MapReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports").RequireAuthorization();

        group.MapGet("/dashboard", async (IReportsModule module, string? format, ExcelExportService excel, PdfExportService pdf, CancellationToken ct) =>
        {
            var data = await module.GetDashboardReportAsync(ct);

            if (format == "excel")
            {
                var list = new List<DashboardReportResponse> { data };
                var bytes = excel.ExportToExcel(list, "Dashboard");
                return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "dashboard_report.xlsx");
            }
            if (format == "pdf")
            {
                var columns = new[] { "TotalUsers", "ActiveUsers", "TotalForms", "PublishedForms", "TotalWorkflows", "PublishedWorkflows", "RunningInstances", "CompletedInstances", "PendingTasks", "CompletedTasks" };
                var bytes = pdf.ExportTableToPdf(new[] { data }, "Dashboard Report", columns,
                    d => new string?[] { d.TotalUsers.ToString(), d.ActiveUsers.ToString(), d.TotalForms.ToString(), d.PublishedForms.ToString(), d.TotalWorkflows.ToString(), d.PublishedWorkflows.ToString(), d.RunningWorkflowInstances.ToString(), d.CompletedWorkflowInstances.ToString(), d.PendingTasks.ToString(), d.CompletedTasks.ToString() });
                return Results.File(bytes, "application/pdf", "dashboard_report.pdf");
            }

            return Results.Ok(data);
        })
        .WithName("GetDashboardReport")
        .WithTags("Reports");

        group.MapGet("/workflows", async (IReportsModule module, string? workflowId, string? status, DateTime? from, DateTime? to, string? startedBy, string? format, ExcelExportService excel, PdfExportService pdf, CancellationToken ct) =>
        {
            var filter = new WorkflowReportFilter(workflowId, status, from, to, startedBy);
            var data = await module.GetWorkflowReportAsync(filter, ct);

            if (format == "excel")
            {
                var bytes = excel.ExportToExcel(data, "WorkflowReport");
                return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "workflow_report.xlsx");
            }
            if (format == "pdf")
            {
                var columns = new[] { "WorkflowName", "Version", "Status", "StartedBy", "StartedDate", "CompletedDate", "DurationHours" };
                var bytes = pdf.ExportTableToPdf(data, "Workflow Report", columns,
                    d => new string?[] { d.WorkflowName, $"v{d.WorkflowVersion}", d.Status, d.StartedBy, d.StartedDate.ToString("yyyy-MM-dd HH:mm"), d.CompletedDate?.ToString("yyyy-MM-dd HH:mm"), d.DurationHours?.ToString("F2") });
                return Results.File(bytes, "application/pdf", "workflow_report.pdf");
            }

            return Results.Ok(data);
        })
        .WithName("GetWorkflowReport")
        .WithTags("Reports");

        group.MapGet("/tasks", async (IReportsModule module, string? userId, string? role, string? status, DateTime? from, DateTime? to, string? format, ExcelExportService excel, PdfExportService pdf, CancellationToken ct) =>
        {
            var filter = new TaskReportFilter(userId, role, status, from, to);
            var data = await module.GetTaskReportAsync(filter, ct);

            if (format == "excel")
            {
                var bytes = excel.ExportToExcel(data, "TaskReport");
                return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "task_report.xlsx");
            }
            if (format == "pdf")
            {
                var columns = new[] { "NodeName", "TaskType", "Title", "AssignedTo", "Status", "CompletedBy", "CompletedDate" };
                var bytes = pdf.ExportTableToPdf(data, "Task Report", columns,
                    d => new string?[] { d.NodeName, d.TaskType, d.Title, d.AssignedToUserId ?? d.AssignedToRole, d.Status, d.CompletedBy, d.CompletedDate?.ToString("yyyy-MM-dd HH:mm") });
                return Results.File(bytes, "application/pdf", "task_report.pdf");
            }

            return Results.Ok(data);
        })
        .WithName("GetTaskReport")
        .WithTags("Reports");

        group.MapGet("/forms", async (IReportsModule module, string? format, ExcelExportService excel, PdfExportService pdf, CancellationToken ct) =>
        {
            var data = await module.GetFormReportAsync(ct);

            if (format == "excel")
            {
                var bytes = excel.ExportToExcel(data.FormUsage, "FormReport");
                return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "form_report.xlsx");
            }
            if (format == "pdf")
            {
                var columns = new[] { "FormName", "UsageCount", "CurrentVersion" };
                var bytes = pdf.ExportTableToPdf(data.FormUsage, "Form Report", columns,
                    d => new string?[] { d.FormName, d.UsageCount.ToString(), d.CurrentVersion.ToString() });
                return Results.File(bytes, "application/pdf", "form_report.pdf");
            }

            return Results.Ok(data);
        })
        .WithName("GetFormReport")
        .WithTags("Reports");

        group.MapGet("/users", async (IReportsModule module, string? userId, DateTime? from, DateTime? to, string? format, ExcelExportService excel, PdfExportService pdf, CancellationToken ct) =>
        {
            var filter = new UserActivityFilter(userId, from, to);
            var data = await module.GetUserActivityReportAsync(filter, ct);

            if (format == "excel")
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook();

                var wsSummary = workbook.Worksheets.Add("Summary");
                wsSummary.Cell(1, 1).Value = "TotalLogins"; wsSummary.Cell(1, 2).Value = "TotalWorkflowsStarted"; wsSummary.Cell(1, 3).Value = "TotalTasksCompleted";
                wsSummary.Cell(2, 1).Value = data.Summary.TotalLogins; wsSummary.Cell(2, 2).Value = data.Summary.TotalWorkflowsStarted; wsSummary.Cell(2, 3).Value = data.Summary.TotalTasksCompleted;

                if (data.LoginHistory.Count > 0)
                {
                    var wsLogin = workbook.Worksheets.Add("LoginHistory");
                    wsLogin.Cell(1, 1).Value = "UserId"; wsLogin.Cell(1, 2).Value = "UserName"; wsLogin.Cell(1, 3).Value = "Timestamp";
                    for (int i = 0; i < data.LoginHistory.Count; i++) { wsLogin.Cell(i + 2, 1).Value = data.LoginHistory[i].UserId; wsLogin.Cell(i + 2, 2).Value = data.LoginHistory[i].UserName; wsLogin.Cell(i + 2, 3).Value = data.LoginHistory[i].Timestamp.ToString("yyyy-MM-dd HH:mm:ss"); }
                }

                if (data.WorkflowParticipations.Count > 0)
                {
                    var wsWf = workbook.Worksheets.Add("WorkflowParticipations");
                    wsWf.Cell(1, 1).Value = "UserId"; wsWf.Cell(1, 2).Value = "WorkflowName"; wsWf.Cell(1, 3).Value = "StartedDate"; wsWf.Cell(1, 4).Value = "CompletedDate"; wsWf.Cell(1, 5).Value = "Status";
                    for (int i = 0; i < data.WorkflowParticipations.Count; i++) { wsWf.Cell(i + 2, 1).Value = data.WorkflowParticipations[i].UserId; wsWf.Cell(i + 2, 2).Value = data.WorkflowParticipations[i].WorkflowName; wsWf.Cell(i + 2, 3).Value = data.WorkflowParticipations[i].StartedDate.ToString("yyyy-MM-dd HH:mm:ss"); wsWf.Cell(i + 2, 4).Value = data.WorkflowParticipations[i].CompletedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""; wsWf.Cell(i + 2, 5).Value = data.WorkflowParticipations[i].Status; }
                }

                if (data.TaskCompletions.Count > 0)
                {
                    var wsTask = workbook.Worksheets.Add("TaskCompletions");
                    wsTask.Cell(1, 1).Value = "UserId"; wsTask.Cell(1, 2).Value = "NodeName"; wsTask.Cell(1, 3).Value = "TaskType"; wsTask.Cell(1, 4).Value = "Status"; wsTask.Cell(1, 5).Value = "CompletedDate";
                    for (int i = 0; i < data.TaskCompletions.Count; i++) { wsTask.Cell(i + 2, 1).Value = data.TaskCompletions[i].UserId; wsTask.Cell(i + 2, 2).Value = data.TaskCompletions[i].NodeName; wsTask.Cell(i + 2, 3).Value = data.TaskCompletions[i].TaskType; wsTask.Cell(i + 2, 4).Value = data.TaskCompletions[i].Status; wsTask.Cell(i + 2, 5).Value = data.TaskCompletions[i].CompletedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""; }
                }

                foreach (var ws in workbook.Worksheets) ws.Columns().AdjustToContents();
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return Results.File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "user_activity_report.xlsx");
            }

            if (format == "pdf")
            {
                var bytes = pdf.ExportTableToPdf(data.LoginHistory, "User Activity - Login History",
                    new[] { "UserId", "UserName", "Timestamp" },
                    d => new string?[] { d.UserId, d.UserName, d.Timestamp.ToString("yyyy-MM-dd HH:mm") });
                return Results.File(bytes, "application/pdf", "user_activity_report.pdf");
            }

            return Results.Ok(data);
        })
        .WithName("GetUserActivityReport")
        .WithTags("Reports");
    }
}