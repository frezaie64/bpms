using System.Security.Claims;
using BPMS.Modules.WorkflowRuntime.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.WorkflowRuntime.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").RequireAuthorization();

        group.MapGet("/", async (IWorkflowRuntimeModule module, CancellationToken ct) =>
            Results.Ok(await module.GetTasksAsync(ct)))
        .WithName("GetTasks")
        .WithTags("Tasks");

        group.MapGet("/{id:guid}", async (Guid id, IWorkflowRuntimeModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.GetTaskByIdAsync(id, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("GetTaskById")
        .WithTags("Tasks");

        group.MapPost("/{id:guid}/approve", async (Guid id, ApproveTaskRequest? request, IWorkflowRuntimeModule module, HttpContext context, CancellationToken ct) =>
        {
            try
            {
                var userId = context.User.FindFirstValue("sub") ?? "unknown";
                await module.ApproveTaskAsync(id, request ?? new ApproveTaskRequest(null), userId, ct);
                return Results.Ok();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("ApproveTask")
        .WithTags("Tasks");

        group.MapPost("/{id:guid}/reject", async (Guid id, RejectTaskRequest? request, IWorkflowRuntimeModule module, HttpContext context, CancellationToken ct) =>
        {
            try
            {
                var userId = context.User.FindFirstValue("sub") ?? "unknown";
                await module.RejectTaskAsync(id, request ?? new RejectTaskRequest(null), userId, ct);
                return Results.Ok();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("RejectTask")
        .WithTags("Tasks");

        group.MapPost("/{id:guid}/submit-form", async (Guid id, SubmitFormTaskRequest request, IWorkflowRuntimeModule module, HttpContext context, CancellationToken ct) =>
        {
            try
            {
                var userId = context.User.FindFirstValue("sub") ?? "unknown";
                await module.SubmitFormTaskAsync(id, request, userId, ct);
                return Results.Ok();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("SubmitFormTask")
        .WithTags("Tasks");
    }
}