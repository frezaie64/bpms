namespace BPMS.Modules.Notifications.Models;

public record NotificationResponse(
    Guid Id,
    string UserId,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt
);

public record CreateNotificationRequest(
    string UserId,
    string Title,
    string Message,
    string Type
);

public record NotificationFilterRequest(
    string? Type,
    bool? IsRead
);