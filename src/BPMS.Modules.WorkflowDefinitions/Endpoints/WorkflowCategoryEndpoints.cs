using BPMS.Modules.WorkflowDefinitions.Models;
using BPMS.Shared.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.WorkflowDefinitions.Endpoints;

public static class WorkflowCategoryEndpoints
{
    public static void MapWorkflowCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workflow-categories").RequireAuthorization();

        group.MapGet("/", async (IWorkflowDefinitionsModule module, CancellationToken ct) =>
            Results.Ok(await module.GetWorkflowCategoriesAsync(ct)))
            .RequireAuthorization(Permissions.Administration.ManageWorkflowCategories)
            .WithName("GetWorkflowCategories")
            .WithTags("Workflow Categories");

        group.MapPost("/", async (CreateWorkflowCategoryRequest request, IWorkflowDefinitionsModule module, CancellationToken ct) =>
        {
            try
            {
                var result = await module.CreateWorkflowCategoryAsync(request, ct);
                return Results.Created($"/api/workflow-categories/{result.Id}", result);
            }
            catch (Exception)
            {
                return Results.Problem("Failed to create workflow category.");
            }
        })
        .RequireAuthorization(Permissions.Administration.ManageWorkflowCategories)
        .WithName("CreateWorkflowCategory")
        .WithTags("Workflow Categories");

        group.MapPut("/{id:guid}", async (Guid id, UpdateWorkflowCategoryRequest request, IWorkflowDefinitionsModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.UpdateWorkflowCategoryAsync(id, request, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .RequireAuthorization(Permissions.Administration.ManageWorkflowCategories)
        .WithName("UpdateWorkflowCategory")
        .WithTags("Workflow Categories");

        group.MapDelete("/{id:guid}", async (Guid id, IWorkflowDefinitionsModule module, CancellationToken ct) =>
        {
            try
            {
                await module.DeleteWorkflowCategoryAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .RequireAuthorization(Permissions.Administration.ManageWorkflowCategories)
        .WithName("DeleteWorkflowCategory")
        .WithTags("Workflow Categories");
    }
}