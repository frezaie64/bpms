using BPMS.Modules.WorkflowRuntime.Entities;

namespace BPMS.Modules.WorkflowRuntime.Models;

public record StartWorkflowRequest(
    Guid WorkflowId
);

public record WorkflowInstanceResponse(
    Guid Id,
    Guid WorkflowId,
    string WorkflowName,
    int WorkflowVersion,
    WorkflowInstanceStatus Status,
    string StartedBy,
    DateTime StartedDate,
    DateTime? CompletedDate,
    DateTime CreatedAt
);

public record WorkflowInstanceDetailResponse(
    Guid Id,
    Guid WorkflowId,
    string WorkflowName,
    int WorkflowVersion,
    WorkflowInstanceStatus Status,
    string StartedBy,
    DateTime StartedDate,
    DateTime? CompletedDate,
    string Variables,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<WorkflowTaskResponse> Tasks,
    List<WorkflowHistoryResponse> History
);

public record WorkflowHistoryResponse(
    Guid Id,
    string EventType,
    string? NodeId,
    string? NodeName,
    string? Data,
    DateTime Timestamp,
    string? PerformedBy
);

public record CancelWorkflowRequest(
    string? Reason
);