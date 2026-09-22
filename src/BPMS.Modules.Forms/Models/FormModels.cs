using BPMS.Modules.Forms.Entities;

namespace BPMS.Modules.Forms.Models;

public record CreateFormRequest(
    string Name,
    string? Description,
    Guid? CategoryId,
    string? JsonDefinition
);

public record UpdateFormRequest(
    string Name,
    string? Description,
    Guid? CategoryId,
    string? JsonDefinition
);

public record FormResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    FormStatus Status,
    int CurrentVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record FormDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    FormStatus Status,
    int CurrentVersion,
    string JsonDefinition,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy,
    List<FormVersionResponse> Versions
);

public record FormVersionResponse(
    Guid Id,
    int VersionNumber,
    FormStatus Status,
    string JsonDefinition,
    DateTime CreatedAt,
    string? Notes
);

public record CreateFormCategoryRequest(
    string Name,
    string? Description
);

public record UpdateFormCategoryRequest(
    string Name,
    string? Description
);

public record FormCategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);