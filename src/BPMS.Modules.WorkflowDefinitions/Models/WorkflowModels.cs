using BPMS.Modules.WorkflowDefinitions.Entities;

namespace BPMS.Modules.WorkflowDefinitions.Models;

public record CreateWorkflowRequest(
    string Name,
    string? Description,
    string? JsonDefinition
);

public record UpdateWorkflowRequest(
    string Name,
    string? Description,
    string? JsonDefinition
);

public record WorkflowResponse(
    Guid Id,
    string Name,
    string? Description,
    WorkflowStatus Status,
    int CurrentVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record WorkflowDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    WorkflowStatus Status,
    int CurrentVersion,
    string JsonDefinition,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy,
    List<WorkflowVersionResponse> Versions
);

public record WorkflowVersionResponse(
    Guid Id,
    int VersionNumber,
    WorkflowStatus Status,
    string JsonDefinition,
    DateTime CreatedAt,
    string? Notes
);