namespace BPMS.Modules.Tenancy.Models;

public record CreateLookupRequest(
    string Group,
    string Name,
    string? Value
);

public record UpdateLookupRequest(
    string Group,
    string Name,
    string? Value,
    bool? IsActive
);

public record LookupResponse(
    Guid Id,
    string Group,
    string Name,
    string? Value,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);