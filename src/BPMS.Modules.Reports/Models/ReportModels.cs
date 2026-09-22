namespace BPMS.Modules.Reports.Models;

public record DashboardReportResponse(
    int TotalUsers,
    int ActiveUsers,
    int TotalForms,
    int PublishedForms,
    int TotalWorkflows,
    int PublishedWorkflows,
    int RunningWorkflowInstances,
    int CompletedWorkflowInstances,
    int PendingTasks,
    int CompletedTasks
);

public record WorkflowReportFilter(
    string? WorkflowId,
    string? Status,
    DateTime? From,
    DateTime? To,
    string? StartedBy
);

public record WorkflowReportItem(
    Guid Id,
    Guid WorkflowId,
    string WorkflowName,
    int WorkflowVersion,
    string Status,
    string StartedBy,
    DateTime StartedDate,
    DateTime? CompletedDate,
    double? DurationHours
);

public record TaskReportFilter(
    string? UserId,
    string? Role,
    string? Status,
    DateTime? From,
    DateTime? To
);

public record TaskReportItem(
    Guid Id,
    Guid WorkflowInstanceId,
    string NodeId,
    string NodeName,
    string TaskType,
    string Title,
    string? AssignedToUserId,
    string? AssignedToRole,
    string Status,
    string? CompletedBy,
    DateTime? CompletedDate,
    string? Comments
);

public record FormReportResponse(
    List<FormUsageItem> FormUsage,
    int TotalSubmissions,
    int UniqueFormsUsed
);

public record FormUsageItem(
    string FormName,
    int UsageCount,
    int CurrentVersion
);

public record UserActivityFilter(
    string? UserId,
    DateTime? From,
    DateTime? To
);

public record UserActivityReportResponse(
    List<UserLoginItem> LoginHistory,
    List<UserWorkflowParticipationItem> WorkflowParticipations,
    List<UserTaskCompletionItem> TaskCompletions,
    UserActivitySummary Summary
);

public record UserLoginItem(
    string UserId,
    string? UserName,
    DateTime Timestamp
);

public record UserWorkflowParticipationItem(
    string UserId,
    string WorkflowName,
    DateTime StartedDate,
    DateTime? CompletedDate,
    string Status
);

public record UserTaskCompletionItem(
    string UserId,
    string NodeName,
    string TaskType,
    string Status,
    DateTime? CompletedDate
);

public record UserActivitySummary(
    int TotalLogins,
    int TotalWorkflowsStarted,
    int TotalTasksCompleted
);