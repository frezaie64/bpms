using BPMS.Modules.Notifications.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.Notifications.Endpoints;

public static class AuditLogEndpoints
{
    public static void MapAuditLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/audit-logs").RequireAuthorization();

        group.MapGet("/", async (string? userId, string? entity, string? eventType, DateTime? from, DateTime? to, INotificationsModule module, CancellationToken ct) =>
        {
            var filter = new AuditLogSearchRequest(userId, entity, eventType, from, to);
            return Results.Ok(await module.GetAuditLogsAsync(filter, ct));
        })
        .WithName("GetAuditLogs")
        .WithTags("Audit Logs");

        group.MapGet("/{id:guid}", async (Guid id, INotificationsModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.GetAuditLogByIdAsync(id, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("GetAuditLogById")
        .WithTags("Audit Logs");
    }
}