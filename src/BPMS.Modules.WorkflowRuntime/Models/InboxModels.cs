using BPMS.Modules.WorkflowRuntime.Entities;

namespace BPMS.Modules.WorkflowRuntime.Models;

public record DashboardResponse(
    int PendingTasks,
    int CompletedTasks,
    int RunningWorkflows,
    int CompletedWorkflows
);

public record InboxTaskResponse(
    Guid Id,
    Guid WorkflowInstanceId,
    string WorkflowName,
    string NodeId,
    string NodeName,
    WorkflowTaskType TaskType,
    string Title,
    WorkflowTaskStatus Status,
    string? CompletedBy,
    DateTime? CompletedDate
);

public record InboxWorkflowInstanceResponse(
    Guid Id,
    Guid WorkflowId,
    string WorkflowName,
    int WorkflowVersion,
    WorkflowInstanceStatus Status,
    DateTime StartedDate,
    DateTime? CompletedDate
);