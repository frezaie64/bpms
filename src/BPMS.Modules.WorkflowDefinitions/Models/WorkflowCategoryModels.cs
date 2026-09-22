namespace BPMS.Modules.WorkflowDefinitions.Models;

public record CreateWorkflowCategoryRequest(
    string Name,
    string? Description
);

public record UpdateWorkflowCategoryRequest(
    string Name,
    string? Description
);

public record WorkflowCategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);