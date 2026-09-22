using BPMS.Modules.WorkflowDefinitions.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.WorkflowDefinitions.Endpoints;

public static class WorkflowEndpoints
{
    public static void MapWorkflowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workflows").RequireAuthorization();

        group.MapGet("/", async (IWorkflowDefinitionsModule module, CancellationToken ct) =>
            Results.Ok(await module.GetWorkflowsAsync(ct)))
            .WithName("GetWorkflows")
            .WithTags("Workflows");

        group.MapGet("/{id:guid}", async (Guid id, IWorkflowDefinitionsModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.GetWorkflowByIdAsync(id, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("GetWorkflowById")
        .WithTags("Workflows");

        group.MapPost("/", async (CreateWorkflowRequest request, IWorkflowDefinitionsModule module, CancellationToken ct) =>
        {
            try
            {
                var result = await module.CreateWorkflowAsync(request, ct);
                return Results.Created($"/api/workflows/{result.Id}", result);
            }
            catch (Exception)
            {
                return Results.Problem("Failed to create workflow.");
            }
        })
        .WithName("CreateWorkflow")
        .WithTags("Workflows");

        group.MapPut("/{id:guid}", async (Guid id, UpdateWorkflowRequest request, IWorkflowDefinitionsModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.UpdateWorkflowAsync(id, request, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("UpdateWorkflow")
        .WithTags("Workflows");

        group.MapDelete("/{id:guid}", async (Guid id, IWorkflowDefinitionsModule module, CancellationToken ct) =>
        {
            try
            {
                await module.DeleteWorkflowAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("DeleteWorkflow")
        .WithTags("Workflows");

        group.MapPost("/{id:guid}/publish", async (Guid id, IWorkflowDefinitionsModule module, CancellationToken ct) =>
        {
            try
            {
                await module.PublishWorkflowAsync(id, ct);
                return Results.Ok();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("PublishWorkflow")
        .WithTags("Workflows");

        group.MapPost("/{id:guid}/archive", async (Guid id, IWorkflowDefinitionsModule module, CancellationToken ct) =>
        {
            try
            {
                await module.ArchiveWorkflowAsync(id, ct);
                return Results.Ok();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("ArchiveWorkflow")
        .WithTags("Workflows");

        group.MapPost("/{id:guid}/duplicate", async (Guid id, IWorkflowDefinitionsModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.DuplicateWorkflowAsync(id, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("DuplicateWorkflow")
        .WithTags("Workflows");

        group.MapGet("/{id:guid}/definition", async (Guid id, IWorkflowDefinitionsModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.GetWorkflowDefinitionAsync(id, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("GetWorkflowDefinition")
        .WithTags("Workflows");
    }
}