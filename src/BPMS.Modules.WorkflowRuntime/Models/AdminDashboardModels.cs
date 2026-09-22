namespace BPMS.Modules.WorkflowRuntime.Models;

public record AdminDashboardResponse(
    int TotalUsers,
    int ActiveUsers,
    int TotalForms,
    int PublishedForms,
    int TotalWorkflows,
    int RunningWorkflowInstances,
    int CompletedWorkflowInstances,
    int PendingTasks
);