using System.Security.Claims;
using BPMS.Modules.WorkflowRuntime.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.WorkflowRuntime.Endpoints;

public static class WorkflowInstanceEndpoints
{
    public static void MapWorkflowInstanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workflow-instances").RequireAuthorization();

        group.MapPost("/", async (StartWorkflowRequest request, IWorkflowRuntimeModule module, HttpContext context, CancellationToken ct) =>
        {
            try
            {
                var userId = context.User.FindFirstValue("sub") ?? "unknown";
                var result = await module.StartWorkflowAsync(request, userId, ct);
                return Results.Created($"/api/workflow-instances/{result.Id}", result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("StartWorkflowInstance")
        .WithTags("Workflow Instances");

        group.MapGet("/", async (IWorkflowRuntimeModule module, CancellationToken ct) =>
            Results.Ok(await module.GetWorkflowInstancesAsync(ct)))
        .WithName("GetWorkflowInstances")
        .WithTags("Workflow Instances");

        group.MapGet("/{id:guid}", async (Guid id, IWorkflowRuntimeModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.GetWorkflowInstanceByIdAsync(id, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("GetWorkflowInstanceById")
        .WithTags("Workflow Instances");

        group.MapPost("/{id:guid}/cancel", async (Guid id, CancelWorkflowRequest? request, IWorkflowRuntimeModule module, CancellationToken ct) =>
        {
            try
            {
                await module.CancelWorkflowAsync(id, request?.Reason, ct);
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
        .WithName("CancelWorkflowInstance")
        .WithTags("Workflow Instances");
    }
}