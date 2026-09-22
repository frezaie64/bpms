using System.Security.Claims;
using BPMS.Modules.WorkflowRuntime.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.WorkflowRuntime.Endpoints;

public static class InboxEndpoints
{
    public static void MapInboxEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inbox").RequireAuthorization();

        group.MapGet("/tasks", async (string? status, IWorkflowRuntimeModule module, HttpContext context, CancellationToken ct) =>
        {
            var userId = context.User.FindFirstValue("sub") ?? "unknown";
            return Results.Ok(await module.GetMyTasksAsync(userId, status, ct));
        })
        .WithName("GetMyTasks")
        .WithTags("Inbox");

        group.MapGet("/workflows", async (string? status, IWorkflowRuntimeModule module, HttpContext context, CancellationToken ct) =>
        {
            var userId = context.User.FindFirstValue("sub") ?? "unknown";
            return Results.Ok(await module.GetMyWorkflowInstancesAsync(userId, status, ct));
        })
        .WithName("GetMyWorkflowInstances")
        .WithTags("Inbox");

        group.MapGet("/dashboard", async (IWorkflowRuntimeModule module, HttpContext context, CancellationToken ct) =>
        {
            var userId = context.User.FindFirstValue("sub") ?? "unknown";
            return Results.Ok(await module.GetDashboardAsync(userId, ct));
        })
        .WithName("GetDashboard")
        .WithTags("Inbox");
    }
}