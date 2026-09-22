using BPMS.Modules.WorkflowRuntime.Entities;

namespace BPMS.Modules.WorkflowRuntime.Models;

public record WorkflowTaskResponse(
    Guid Id,
    Guid WorkflowInstanceId,
    string NodeId,
    string NodeName,
    WorkflowTaskType TaskType,
    string Title,
    string? AssignedToUserId,
    string? AssignedToRole,
    WorkflowTaskStatus Status,
    string? CompletedBy,
    DateTime? CompletedDate,
    string? Comments,
    string? FormData
);

public record ApproveTaskRequest(
    string? Comments
);

public record RejectTaskRequest(
    string? Comments
);

public record SubmitFormTaskRequest(
    string FormData,
    string? Comments
);