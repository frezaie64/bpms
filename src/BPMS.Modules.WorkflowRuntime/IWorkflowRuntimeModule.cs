using BPMS.Modules.WorkflowRuntime.Models;

namespace BPMS.Modules.WorkflowRuntime;

public interface IWorkflowRuntimeModule
{
    Task<WorkflowInstanceResponse> StartWorkflowAsync(StartWorkflowRequest request, string userId, CancellationToken ct = default);
    Task<List<WorkflowInstanceResponse>> GetWorkflowInstancesAsync(CancellationToken ct = default);
    Task<WorkflowInstanceDetailResponse> GetWorkflowInstanceByIdAsync(Guid id, CancellationToken ct = default);
    Task CancelWorkflowAsync(Guid id, string? reason, CancellationToken ct = default);
    Task<List<WorkflowTaskResponse>> GetTasksAsync(CancellationToken ct = default);
    Task<WorkflowTaskResponse> GetTaskByIdAsync(Guid id, CancellationToken ct = default);
    Task ApproveTaskAsync(Guid id, ApproveTaskRequest request, string userId, CancellationToken ct = default);
    Task RejectTaskAsync(Guid id, RejectTaskRequest request, string userId, CancellationToken ct = default);
    Task SubmitFormTaskAsync(Guid id, SubmitFormTaskRequest request, string userId, CancellationToken ct = default);

    Task<List<InboxTaskResponse>> GetMyTasksAsync(string userId, string? status, CancellationToken ct = default);
    Task<List<InboxWorkflowInstanceResponse>> GetMyWorkflowInstancesAsync(string userId, string? status, CancellationToken ct = default);
    Task<DashboardResponse> GetDashboardAsync(string userId, CancellationToken ct = default);
    Task<Models.AdminDashboardResponse> GetAdminDashboardAsync(CancellationToken ct = default);
}