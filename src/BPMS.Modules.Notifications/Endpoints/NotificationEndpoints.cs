using System.Security.Claims;
using BPMS.Modules.Notifications.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.Notifications.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications").RequireAuthorization();

        group.MapGet("/", async (string? type, bool? isRead, INotificationsModule module, HttpContext context, CancellationToken ct) =>
        {
            var userId = context.User.FindFirstValue("sub") ?? "unknown";
            var filter = new NotificationFilterRequest(type, isRead);
            return Results.Ok(await module.GetNotificationsAsync(userId, filter, ct));
        })
        .WithName("GetNotifications")
        .WithTags("Notifications");

        group.MapGet("/{id:guid}", async (Guid id, INotificationsModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.GetNotificationByIdAsync(id, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("GetNotificationById")
        .WithTags("Notifications");

        group.MapPut("/{id:guid}/read", async (Guid id, INotificationsModule module, CancellationToken ct) =>
        {
            try
            {
                await module.MarkAsReadAsync(id, ct);
                return Results.Ok();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("MarkNotificationAsRead")
        .WithTags("Notifications");

        group.MapPut("/read-all", async (INotificationsModule module, HttpContext context, CancellationToken ct) =>
        {
            var userId = context.User.FindFirstValue("sub") ?? "unknown";
            await module.MarkAllAsReadAsync(userId, ct);
            return Results.Ok();
        })
        .WithName("MarkAllNotificationsAsRead")
        .WithTags("Notifications");

        group.MapDelete("/{id:guid}", async (Guid id, INotificationsModule module, CancellationToken ct) =>
        {
            try
            {
                await module.DeleteNotificationAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("DeleteNotification")
        .WithTags("Notifications");
    }
}