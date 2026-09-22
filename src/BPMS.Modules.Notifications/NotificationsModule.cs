using BPMS.Modules.Notifications.Entities;
using BPMS.Modules.Notifications.Models;
using BPMS.Shared.Abstractions;
using BPMS.Shared.Persistence;
using BPMS.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BPMS.Modules.Notifications;

public class NotificationsModule : INotificationsModule
{
    private readonly BpmsDbContext _db;
    private readonly ITenantService _tenant;

    public NotificationsModule(BpmsDbContext db, ITenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<List<NotificationResponse>> GetNotificationsAsync(string userId, NotificationFilterRequest? filter, CancellationToken ct)
    {
        var query = _db.Set<Notification>()
            .Where(n => n.UserId == userId)
            .AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.Type))
                query = query.Where(n => n.Type == filter.Type);

            if (filter.IsRead.HasValue)
                query = query.Where(n => n.IsRead == filter.IsRead.Value);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

        return notifications.Select(MapNotificationResponse).ToList();
    }

    public async Task<NotificationResponse> GetNotificationByIdAsync(Guid id, CancellationToken ct)
    {
        var notification = await _db.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new KeyNotFoundException($"Notification with ID {id} not found.");

        return MapNotificationResponse(notification);
    }

    public async Task MarkAsReadAsync(Guid id, CancellationToken ct)
    {
        var notification = await _db.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new KeyNotFoundException($"Notification with ID {id} not found.");

        notification.IsRead = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken ct)
    {
        var unread = await _db.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var notification in unread)
            notification.IsRead = true;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteNotificationAsync(Guid id, CancellationToken ct)
    {
        var notification = await _db.Set<Notification>()
            .FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new KeyNotFoundException($"Notification with ID {id} not found.");

        _db.Set<Notification>().Remove(notification);
        await _db.SaveChangesAsync(ct);
    }

    public async Task CreateNotificationAsync(CreateNotificationRequest request, CancellationToken ct)
    {
        var notification = new Notification
        {
            TenantId = _tenant.TenantId,
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Set<Notification>().Add(notification);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<AuditLogResponse>> GetAuditLogsAsync(AuditLogSearchRequest? filter, CancellationToken ct)
    {
        var query = _db.Set<AuditLog>().AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.UserId))
                query = query.Where(a => a.UserId == filter.UserId);

            if (!string.IsNullOrEmpty(filter.Entity))
                query = query.Where(a => a.Entity == filter.Entity);

            if (!string.IsNullOrEmpty(filter.Event))
                query = query.Where(a => a.Event == filter.Event);

            if (filter.From.HasValue)
                query = query.Where(a => a.Timestamp >= filter.From.Value);

            if (filter.To.HasValue)
                query = query.Where(a => a.Timestamp <= filter.To.Value);
        }

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(ct);

        return logs.Select(MapAuditLogResponse).ToList();
    }

    public async Task<AuditLogResponse> GetAuditLogByIdAsync(Guid id, CancellationToken ct)
    {
        var log = await _db.Set<AuditLog>()
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new KeyNotFoundException($"Audit log with ID {id} not found.");

        return MapAuditLogResponse(log);
    }

    public async Task CreateAuditLogAsync(string eventName, string userId, string? userName, string entity, string? entityId, string? details, CancellationToken ct)
    {
        var log = new AuditLog
        {
            TenantId = _tenant.TenantId,
            Event = eventName,
            UserId = userId,
            UserName = userName,
            Entity = entity,
            EntityId = entityId,
            Timestamp = DateTime.UtcNow,
            Details = details
        };

        _db.Set<AuditLog>().Add(log);
        await _db.SaveChangesAsync(ct);
    }

    private static NotificationResponse MapNotificationResponse(Notification notification)
    {
        return new NotificationResponse(
            notification.Id,
            notification.UserId,
            notification.Title,
            notification.Message,
            notification.Type,
            notification.IsRead,
            notification.CreatedAt
        );
    }

    private static AuditLogResponse MapAuditLogResponse(AuditLog log)
    {
        return new AuditLogResponse(
            log.Id,
            log.Event,
            log.UserId,
            log.UserName,
            log.Entity,
            log.EntityId,
            log.Timestamp,
            log.Details
        );
    }
}