namespace BPMS.Modules.WorkflowDefinitions.Models;

public record SubscribeRequest(
    Guid WorkflowId,
    Guid UserId
);

public record WorkflowSubscriptionResponse(
    Guid Id,
    Guid WorkflowId,
    Guid UserId
);

public record CatalogWorkflowResponse(
    Guid Id,
    string Name,
    string? Description,
    int CurrentVersion,
    DateTime CreatedAt
);