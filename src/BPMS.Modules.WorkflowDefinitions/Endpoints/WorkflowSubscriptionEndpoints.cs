using System.Security.Claims;
using BPMS.Modules.WorkflowDefinitions.Models;
using BPMS.Shared.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.WorkflowDefinitions.Endpoints;

public static class WorkflowSubscriptionEndpoints
{
    public static void MapWorkflowSubscriptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workflow-subscriptions").RequireAuthorization();

        group.MapGet("/", async (IWorkflowDefinitionsModule module, CancellationToken ct) =>
            Results.Ok(await module.GetWorkflowsAsync(ct)))
            .WithName("GetWorkflowSubscriptions")
            .WithTags("Workflow Subscriptions")
            .RequireAuthorization(Permissions.WorkflowSubscriptions.Manage);

        group.MapPost("/", async (SubscribeRequest request, IWorkflowDefinitionsModule module, CancellationToken ct) =>
        {
            try
            {
                var result = await module.SubscribeUserAsync(request.WorkflowId, request.UserId, ct);
                return Results.Created($"/api/workflow-subscriptions", result);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateWorkflowSubscription")
        .WithTags("Workflow Subscriptions")
        .RequireAuthorization(Permissions.WorkflowSubscriptions.Manage);

        group.MapDelete("/{id:guid}", async (Guid id, IWorkflowDefinitionsModule module, CancellationToken ct) =>
        {
            try
            {
                await module.UnsubscribeUserAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("DeleteWorkflowSubscription")
        .WithTags("Workflow Subscriptions")
        .RequireAuthorization(Permissions.WorkflowSubscriptions.Manage);
    }
}