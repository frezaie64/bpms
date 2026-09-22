using BPMS.Modules.Reports.Models;
using BPMS.Shared.Persistence;
using BPMS.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BPMS.Modules.Reports;

public class ReportsModule : IReportsModule
{
    private readonly BpmsDbContext _db;
    private readonly ITenantService _tenant;

    public ReportsModule(BpmsDbContext db, ITenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<DashboardReportResponse> GetDashboardReportAsync(CancellationToken ct)
    {
        var tenantId = _tenant.TenantId;

        var totalUsers = await _db.Set<global::BPMS.Modules.Identity.Entities.User>()
            .CountAsync(u => u.TenantId == tenantId && !u.IsDeleted, ct);
        var activeUsers = await _db.Set<global::BPMS.Modules.Identity.Entities.User>()
            .CountAsync(u => u.TenantId == tenantId && u.Status == global::BPMS.Modules.Identity.Entities.UserStatus.Active && !u.IsDeleted, ct);
        var totalForms = await _db.Set<global::BPMS.Modules.Forms.Entities.Form>()
            .CountAsync(f => f.TenantId == tenantId && !f.IsDeleted, ct);
        var publishedForms = await _db.Set<global::BPMS.Modules.Forms.Entities.Form>()
            .CountAsync(f => f.TenantId == tenantId && f.Status == global::BPMS.Modules.Forms.Entities.FormStatus.Published && !f.IsDeleted, ct);
        var totalWorkflows = await _db.Set<global::BPMS.Modules.WorkflowDefinitions.Entities.Workflow>()
            .CountAsync(w => w.TenantId == tenantId && !w.IsDeleted, ct);
        var publishedWorkflows = await _db.Set<global::BPMS.Modules.WorkflowDefinitions.Entities.Workflow>()
            .CountAsync(w => w.TenantId == tenantId && w.Status == global::BPMS.Modules.WorkflowDefinitions.Entities.WorkflowStatus.Published && !w.IsDeleted, ct);
        var runningInstances = await _db.Set<global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowInstance>()
            .CountAsync(i => i.TenantId == tenantId && i.Status == global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowInstanceStatus.Running, ct);
        var completedInstances = await _db.Set<global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowInstance>()
            .CountAsync(i => i.TenantId == tenantId && i.Status == global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowInstanceStatus.Completed, ct);
        var pendingTasks = await _db.Set<global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowTask>()
            .CountAsync(t => t.TenantId == tenantId && t.Status == global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowTaskStatus.Pending, ct);
        var completedTasks = await _db.Set<global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowTask>()
            .CountAsync(t => t.TenantId == tenantId && t.Status != global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowTaskStatus.Pending, ct);

        return new DashboardReportResponse(
            totalUsers, activeUsers, totalForms, publishedForms,
            totalWorkflows, publishedWorkflows, runningInstances, completedInstances,
            pendingTasks, completedTasks
        );
    }

    public async Task<List<WorkflowReportItem>> GetWorkflowReportAsync(WorkflowReportFilter filter, CancellationToken ct)
    {
        var query = _db.Set<global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowInstance>().AsQueryable();

        if (!string.IsNullOrEmpty(filter.WorkflowId) && Guid.TryParse(filter.WorkflowId, out var workflowId))
            query = query.Where(i => i.WorkflowId == workflowId);

        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowInstanceStatus>(filter.Status, true, out var status))
            query = query.Where(i => i.Status == status);

        if (filter.From.HasValue)
            query = query.Where(i => i.StartedDate >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(i => i.StartedDate <= filter.To.Value);

        if (!string.IsNullOrEmpty(filter.StartedBy))
            query = query.Where(i => i.StartedBy == filter.StartedBy);

        var instances = await query
            .OrderByDescending(i => i.StartedDate)
            .ToListAsync(ct);

        return instances.Select(i => new WorkflowReportItem(
            i.Id,
            i.WorkflowId,
            i.WorkflowName,
            i.WorkflowVersion,
            i.Status.ToString(),
            i.StartedBy,
            i.StartedDate,
            i.CompletedDate,
            i.CompletedDate.HasValue ? (i.CompletedDate.Value - i.StartedDate).TotalHours : null
        )).ToList();
    }

    public async Task<List<TaskReportItem>> GetTaskReportAsync(TaskReportFilter filter, CancellationToken ct)
    {
        var query = _db.Set<global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowTask>().AsQueryable();

        if (!string.IsNullOrEmpty(filter.UserId))
            query = query.Where(t => t.AssignedToUserId == filter.UserId || t.CompletedBy == filter.UserId);

        if (!string.IsNullOrEmpty(filter.Role))
            query = query.Where(t => t.AssignedToRole == filter.Role);

        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowTaskStatus>(filter.Status, true, out var taskStatus))
            query = query.Where(t => t.Status == taskStatus);

        if (filter.From.HasValue)
            query = query.Where(t => t.CompletedDate >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(t => t.CompletedDate <= filter.To.Value);

        var tasks = await query
            .OrderByDescending(t => t.CompletedDate ?? DateTime.MinValue)
            .ToListAsync(ct);

        return tasks.Select(t => new TaskReportItem(
            t.Id,
            t.WorkflowInstanceId,
            t.NodeId,
            t.NodeName,
            t.TaskType.ToString(),
            t.Title,
            t.AssignedToUserId,
            t.AssignedToRole,
            t.Status.ToString(),
            t.CompletedBy,
            t.CompletedDate,
            t.Comments
        )).ToList();
    }

    public async Task<FormReportResponse> GetFormReportAsync(CancellationToken ct)
    {
        var tenantId = _tenant.TenantId;

        var forms = await _db.Set<global::BPMS.Modules.Forms.Entities.Form>()
            .Where(f => f.TenantId == tenantId && !f.IsDeleted)
            .ToListAsync(ct);

        var fillFormTasks = await _db.Set<global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowTask>()
            .Where(t => t.TenantId == tenantId && t.TaskType == global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowTaskType.FillForm)
            .ToListAsync(ct);

        var formNameCounts = fillFormTasks
            .GroupBy(t => t.NodeName)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var formUsage = forms
            .Select(f => new FormUsageItem(
                f.Name,
                formNameCounts.GetValueOrDefault(f.Name, 0),
                f.CurrentVersion
            ))
            .OrderByDescending(f => f.UsageCount)
            .ToList();

        return new FormReportResponse(
            formUsage,
            fillFormTasks.Count,
            formUsage.Count(f => f.UsageCount > 0)
        );
    }

    public async Task<UserActivityReportResponse> GetUserActivityReportAsync(UserActivityFilter filter, CancellationToken ct)
    {
        var loginQuery = _db.Set<global::BPMS.Modules.Notifications.Entities.AuditLog>()
            .Where(a => a.Event.Contains("Login") || a.Entity == "User")
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.UserId))
            loginQuery = loginQuery.Where(a => a.UserId == filter.UserId);
        if (filter.From.HasValue)
            loginQuery = loginQuery.Where(a => a.Timestamp >= filter.From.Value);
        if (filter.To.HasValue)
            loginQuery = loginQuery.Where(a => a.Timestamp <= filter.To.Value);

        var loginHistory = await loginQuery
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new UserLoginItem(a.UserId, a.UserName, a.Timestamp))
            .Take(100)
            .ToListAsync(ct);

        var workflowQuery = _db.Set<global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowInstance>().AsQueryable();

        if (!string.IsNullOrEmpty(filter.UserId))
            workflowQuery = workflowQuery.Where(i => i.StartedBy == filter.UserId);
        if (filter.From.HasValue)
            workflowQuery = workflowQuery.Where(i => i.StartedDate >= filter.From.Value);
        if (filter.To.HasValue)
            workflowQuery = workflowQuery.Where(i => i.StartedDate <= filter.To.Value);

        var workflowParticipation = await workflowQuery
            .OrderByDescending(i => i.StartedDate)
            .Select(i => new UserWorkflowParticipationItem(i.StartedBy, i.WorkflowName, i.StartedDate, i.CompletedDate, i.Status.ToString()))
            .Take(100)
            .ToListAsync(ct);

        var taskQuery = _db.Set<global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowTask>()
            .Where(t => t.CompletedBy != null)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.UserId))
            taskQuery = taskQuery.Where(t => t.CompletedBy == filter.UserId);
        if (filter.From.HasValue)
            taskQuery = taskQuery.Where(t => t.CompletedDate >= filter.From.Value);
        if (filter.To.HasValue)
            taskQuery = taskQuery.Where(t => t.CompletedDate <= filter.To.Value);

        var taskCompletions = await taskQuery
            .OrderByDescending(t => t.CompletedDate)
            .Select(t => new UserTaskCompletionItem(t.CompletedBy!, t.NodeName, t.TaskType.ToString(), t.Status.ToString(), t.CompletedDate))
            .Take(100)
            .ToListAsync(ct);

        var totalLogins = await loginQuery.CountAsync(ct);
        var totalWorkflowsStarted = await workflowQuery.CountAsync(ct);
        var totalTasksCompleted = await taskQuery.CountAsync(ct);

        return new UserActivityReportResponse(
            loginHistory,
            workflowParticipation,
            taskCompletions,
            new UserActivitySummary(totalLogins, totalWorkflowsStarted, totalTasksCompleted)
        );
    }

    public async Task<SearchResponse> SearchAsync(string query, CancellationToken ct)
    {
        var results = new List<SearchResultItem>();
        var lowerQuery = query.ToLowerInvariant();

        var workflows = await _db.Set<global::BPMS.Modules.WorkflowDefinitions.Entities.Workflow>()
            .Where(w => !w.IsDeleted &&
                (EF.Functions.ILike(w.Name, $"%{query}%") || (w.Description != null && EF.Functions.ILike(w.Description, $"%{query}%"))))
            .Take(10)
            .ToListAsync(ct);

        results.AddRange(workflows.Select(w => new SearchResultItem("Workflow", w.Id, w.Name, w.Description, w.Status.ToString())));

        var instances = await _db.Set<global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowInstance>()
            .Where(i => EF.Functions.ILike(i.WorkflowName, $"%{query}%"))
            .Take(10)
            .ToListAsync(ct);

        results.AddRange(instances.Select(i => new SearchResultItem("WorkflowInstance", i.Id, i.WorkflowName, $"v{i.WorkflowVersion}", i.Status.ToString())));

        var tasks = await _db.Set<global::BPMS.Modules.WorkflowRuntime.Entities.WorkflowTask>()
            .Where(t => EF.Functions.ILike(t.Title, $"%{query}%") || EF.Functions.ILike(t.NodeName, $"%{query}%"))
            .Take(10)
            .ToListAsync(ct);

        results.AddRange(tasks.Select(t => new SearchResultItem("Task", t.Id, t.Title, t.NodeName, t.Status.ToString())));

        var forms = await _db.Set<global::BPMS.Modules.Forms.Entities.Form>()
            .Where(f => !f.IsDeleted &&
                (EF.Functions.ILike(f.Name, $"%{query}%") || (f.Description != null && EF.Functions.ILike(f.Description, $"%{query}%"))))
            .Take(10)
            .ToListAsync(ct);

        results.AddRange(forms.Select(f => new SearchResultItem("Form", f.Id, f.Name, f.Description, f.Status.ToString())));

        var users = await _db.Set<global::BPMS.Modules.Identity.Entities.User>()
            .Where(u => !u.IsDeleted &&
                (EF.Functions.ILike(u.Email, $"%{query}%") ||
                 EF.Functions.ILike(u.FirstName, $"%{query}%") ||
                 EF.Functions.ILike(u.LastName, $"%{query}%")))
            .Take(10)
            .ToListAsync(ct);

        results.AddRange(users.Select(u => new SearchResultItem("User", u.Id, $"{u.FirstName} {u.LastName}", u.Email, u.Status.ToString())));

        return new SearchResponse(results, results.Count);
    }
}