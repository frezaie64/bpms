using BPMS.Modules.WorkflowRuntime.Entities;
using BPMS.Modules.WorkflowRuntime.Models;
using BPMS.Modules.WorkflowRuntime.Services;
using BPMS.Shared.Abstractions;
using BPMS.Shared.Persistence;
using BPMS.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BPMS.Modules.WorkflowRuntime;

public class WorkflowRuntimeModule : IWorkflowRuntimeModule
{
    private readonly BpmsDbContext _db;
    private readonly WorkflowEngine _engine;
    private readonly ITenantService _tenant;

    public WorkflowRuntimeModule(BpmsDbContext db, WorkflowEngine engine, ITenantService tenant)
    {
        _db = db;
        _engine = engine;
        _tenant = tenant;
    }

    public async Task<WorkflowInstanceResponse> StartWorkflowAsync(StartWorkflowRequest request, string userId, CancellationToken ct)
    {
        var instance = await _engine.StartWorkflowAsync(request.WorkflowId, userId, ct);
        return MapInstanceResponse(instance);
    }

    public async Task<List<WorkflowInstanceResponse>> GetWorkflowInstancesAsync(CancellationToken ct)
    {
        var instances = await _db.Set<WorkflowInstance>()
            .OrderByDescending(i => i.StartedDate)
            .ToListAsync(ct);

        return instances.Select(MapInstanceResponse).ToList();
    }

    public async Task<WorkflowInstanceDetailResponse> GetWorkflowInstanceByIdAsync(Guid id, CancellationToken ct)
    {
        var instance = await _db.Set<WorkflowInstance>()
            .Include(i => i.Tasks)
            .Include(i => i.History.OrderBy(h => h.Timestamp))
            .FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new KeyNotFoundException($"Workflow instance {id} not found.");

        return MapInstanceDetailResponse(instance);
    }

    public async Task CancelWorkflowAsync(Guid id, string? reason, CancellationToken ct)
    {
        await _engine.CancelWorkflowAsync(id, reason);
    }

    public async Task<List<WorkflowTaskResponse>> GetTasksAsync(CancellationToken ct)
    {
        var tasks = await _db.Set<WorkflowTask>()
            .OrderByDescending(t => t.CompletedDate ?? t.CompletedDate ?? DateTime.MinValue)
            .ToListAsync(ct);

        return tasks.Select(MapTaskResponse).ToList();
    }

    public async Task<WorkflowTaskResponse> GetTaskByIdAsync(Guid id, CancellationToken ct)
    {
        var task = await _db.Set<WorkflowTask>()
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new KeyNotFoundException($"Task {id} not found.");

        return MapTaskResponse(task);
    }

    public async Task ApproveTaskAsync(Guid id, ApproveTaskRequest request, string userId, CancellationToken ct)
    {
        await _engine.CompleteTaskAsync(id, WorkflowTaskStatus.Approved, userId, request.Comments, null, ct);
    }

    public async Task RejectTaskAsync(Guid id, RejectTaskRequest request, string userId, CancellationToken ct)
    {
        await _engine.CompleteTaskAsync(id, WorkflowTaskStatus.Rejected, userId, request.Comments, null, ct);
    }

    public async Task SubmitFormTaskAsync(Guid id, SubmitFormTaskRequest request, string userId, CancellationToken ct)
    {
        await _engine.CompleteTaskAsync(id, WorkflowTaskStatus.Submitted, userId, request.Comments, request.FormData, ct);
    }

    public async Task<List<InboxTaskResponse>> GetMyTasksAsync(string userId, string? status, CancellationToken ct)
    {
        var query = _db.Set<WorkflowTask>()
            .Where(t => t.AssignedToUserId == userId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<WorkflowTaskStatus>(status, true, out var taskStatus))
            query = query.Where(t => t.Status == taskStatus);

        var tasks = await query
            .OrderByDescending(t => t.CompletedDate ?? DateTime.MinValue)
            .ToListAsync(ct);

        var instanceIds = tasks.Select(t => t.WorkflowInstanceId).Distinct().ToList();
        var instances = await _db.Set<WorkflowInstance>()
            .Where(i => instanceIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => i.WorkflowName, ct);

        return tasks.Select(t => new InboxTaskResponse(
            t.Id,
            t.WorkflowInstanceId,
            instances.GetValueOrDefault(t.WorkflowInstanceId, "Unknown"),
            t.NodeId,
            t.NodeName,
            t.TaskType,
            t.Title,
            t.Status,
            t.CompletedBy,
            t.CompletedDate
        )).ToList();
    }

    public async Task<List<InboxWorkflowInstanceResponse>> GetMyWorkflowInstancesAsync(string userId, string? status, CancellationToken ct)
    {
        var query = _db.Set<WorkflowInstance>()
            .Where(i => i.StartedBy == userId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<WorkflowInstanceStatus>(status, true, out var instanceStatus))
            query = query.Where(i => i.Status == instanceStatus);

        var instances = await query
            .OrderByDescending(i => i.StartedDate)
            .ToListAsync(ct);

        return instances.Select(i => new InboxWorkflowInstanceResponse(
            i.Id,
            i.WorkflowId,
            i.WorkflowName,
            i.WorkflowVersion,
            i.Status,
            i.StartedDate,
            i.CompletedDate
        )).ToList();
    }

    public async Task<DashboardResponse> GetDashboardAsync(string userId, CancellationToken ct)
    {
        var pendingTasks = await _db.Set<WorkflowTask>()
            .CountAsync(t => t.AssignedToUserId == userId && t.Status == WorkflowTaskStatus.Pending, ct);

        var completedTasks = await _db.Set<WorkflowTask>()
            .CountAsync(t => t.AssignedToUserId == userId && t.Status != WorkflowTaskStatus.Pending, ct);

        var runningWorkflows = await _db.Set<WorkflowInstance>()
            .CountAsync(i => i.StartedBy == userId && i.Status == WorkflowInstanceStatus.Running, ct);

        var completedWorkflows = await _db.Set<WorkflowInstance>()
            .CountAsync(i => i.StartedBy == userId && i.Status == WorkflowInstanceStatus.Completed, ct);

        return new DashboardResponse(pendingTasks, completedTasks, runningWorkflows, completedWorkflows);
    }

    public async Task<AdminDashboardResponse> GetAdminDashboardAsync(CancellationToken ct)
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
        var runningInstances = await _db.Set<WorkflowInstance>()
            .CountAsync(i => i.TenantId == tenantId && i.Status == WorkflowInstanceStatus.Running, ct);
        var completedInstances = await _db.Set<WorkflowInstance>()
            .CountAsync(i => i.TenantId == tenantId && i.Status == WorkflowInstanceStatus.Completed, ct);
        var pendingTasks = await _db.Set<WorkflowTask>()
            .CountAsync(t => t.TenantId == tenantId && t.Status == WorkflowTaskStatus.Pending, ct);

        return new AdminDashboardResponse(
            totalUsers, activeUsers, totalForms, publishedForms,
            totalWorkflows, runningInstances, completedInstances, pendingTasks
        );
    }

    private static WorkflowInstanceResponse MapInstanceResponse(WorkflowInstance instance)
    {
        return new WorkflowInstanceResponse(
            instance.Id,
            instance.WorkflowId,
            instance.WorkflowName,
            instance.WorkflowVersion,
            instance.Status,
            instance.StartedBy,
            instance.StartedDate,
            instance.CompletedDate,
            instance.CreatedAt
        );
    }

    private static WorkflowInstanceDetailResponse MapInstanceDetailResponse(WorkflowInstance instance)
    {
        return new WorkflowInstanceDetailResponse(
            instance.Id,
            instance.WorkflowId,
            instance.WorkflowName,
            instance.WorkflowVersion,
            instance.Status,
            instance.StartedBy,
            instance.StartedDate,
            instance.CompletedDate,
            instance.Variables,
            instance.CreatedAt,
            instance.UpdatedAt,
            instance.Tasks.Select(MapTaskResponse).ToList(),
            instance.History.Select(h => new WorkflowHistoryResponse(
                h.Id,
                h.EventType,
                h.NodeId,
                h.NodeName,
                h.Data,
                h.Timestamp,
                h.PerformedBy
            )).ToList()
        );
    }

    private static WorkflowTaskResponse MapTaskResponse(WorkflowTask task)
    {
        return new WorkflowTaskResponse(
            task.Id,
            task.WorkflowInstanceId,
            task.NodeId,
            task.NodeName,
            task.TaskType,
            task.Title,
            task.AssignedToUserId,
            task.AssignedToRole,
            task.Status,
            task.CompletedBy,
            task.CompletedDate,
            task.Comments,
            task.FormData
        );
    }
}