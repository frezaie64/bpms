using BPMS.Modules.Forms.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BPMS.Modules.Forms.Endpoints;

public static class FormEndpoints
{
    public static void MapFormEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/forms").RequireAuthorization();

        group.MapGet("/", async (IFormsModule module, CancellationToken ct) =>
            Results.Ok(await module.GetFormsAsync(ct)))
            .WithName("GetForms")
            .WithTags("Forms");

        group.MapGet("/{id:guid}", async (Guid id, IFormsModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.GetFormByIdAsync(id, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("GetFormById")
        .WithTags("Forms");

        group.MapPost("/", async (CreateFormRequest request, IFormsModule module, CancellationToken ct) =>
        {
            try
            {
                var result = await module.CreateFormAsync(request, ct);
                return Results.Created($"/api/forms/{result.Id}", result);
            }
            catch (Exception)
            {
                return Results.Problem("Failed to create form.");
            }
        })
        .WithName("CreateForm")
        .WithTags("Forms");

        group.MapPut("/{id:guid}", async (Guid id, UpdateFormRequest request, IFormsModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.UpdateFormAsync(id, request, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("UpdateForm")
        .WithTags("Forms");

        group.MapDelete("/{id:guid}", async (Guid id, IFormsModule module, CancellationToken ct) =>
        {
            try
            {
                await module.DeleteFormAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("DeleteForm")
        .WithTags("Forms");

        group.MapPost("/{id:guid}/publish", async (Guid id, IFormsModule module, CancellationToken ct) =>
        {
            try
            {
                await module.PublishFormAsync(id, ct);
                return Results.Ok();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("PublishForm")
        .WithTags("Forms");

        group.MapPost("/{id:guid}/archive", async (Guid id, IFormsModule module, CancellationToken ct) =>
        {
            try
            {
                await module.ArchiveFormAsync(id, ct);
                return Results.Ok();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("ArchiveForm")
        .WithTags("Forms");

        group.MapPost("/{id:guid}/duplicate", async (Guid id, IFormsModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.DuplicateFormAsync(id, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("DuplicateForm")
        .WithTags("Forms");

        group.MapGet("/{id:guid}/preview", async (Guid id, IFormsModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.PreviewFormAsync(id, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("PreviewForm")
        .WithTags("Forms");
    }
}