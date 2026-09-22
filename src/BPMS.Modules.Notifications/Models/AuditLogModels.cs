namespace BPMS.Modules.Notifications.Models;

public record AuditLogResponse(
    Guid Id,
    string Event,
    string UserId,
    string? UserName,
    string Entity,
    string? EntityId,
    DateTime Timestamp,
    string? Details
);

public record AuditLogSearchRequest(
    string? UserId,
    string? Entity,
    string? Event,
    DateTime? From,
    DateTime? To
);