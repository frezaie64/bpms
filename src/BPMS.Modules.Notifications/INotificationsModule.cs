using BPMS.Modules.Notifications.Models;

namespace BPMS.Modules.Notifications;

public interface INotificationsModule
{
    Task<List<NotificationResponse>> GetNotificationsAsync(string userId, NotificationFilterRequest? filter, CancellationToken ct = default);
    Task<NotificationResponse> GetNotificationByIdAsync(Guid id, CancellationToken ct = default);
    Task MarkAsReadAsync(Guid id, CancellationToken ct = default);
    Task MarkAllAsReadAsync(string userId, CancellationToken ct = default);
    Task DeleteNotificationAsync(Guid id, CancellationToken ct = default);
    Task CreateNotificationAsync(CreateNotificationRequest request, CancellationToken ct = default);

    Task<List<AuditLogResponse>> GetAuditLogsAsync(AuditLogSearchRequest? filter, CancellationToken ct = default);
    Task<AuditLogResponse> GetAuditLogByIdAsync(Guid id, CancellationToken ct = default);
    Task CreateAuditLogAsync(string eventName, string userId, string? userName, string entity, string? entityId, string? details, CancellationToken ct = default);
}