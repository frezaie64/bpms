using BPMS.Modules.Forms.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.Forms.Endpoints;

public static class FormCategoryEndpoints
{
    public static void MapFormCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/form-categories").RequireAuthorization();

        group.MapGet("/", async (IFormsModule module, CancellationToken ct) =>
            Results.Ok(await module.GetCategoriesAsync(ct)))
            .WithName("GetFormCategories")
            .WithTags("Form Categories");

        group.MapPost("/", async (CreateFormCategoryRequest request, IFormsModule module, CancellationToken ct) =>
        {
            try
            {
                var result = await module.CreateCategoryAsync(request, ct);
                return Results.Created($"/api/form-categories/{result.Id}", result);
            }
            catch (Exception)
            {
                return Results.Problem("Failed to create category.");
            }
        })
        .WithName("CreateFormCategory")
        .WithTags("Form Categories");

        group.MapPut("/{id:guid}", async (Guid id, UpdateFormCategoryRequest request, IFormsModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.UpdateCategoryAsync(id, request, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("UpdateFormCategory")
        .WithTags("Form Categories");

        group.MapDelete("/{id:guid}", async (Guid id, IFormsModule module, CancellationToken ct) =>
        {
            try
            {
                await module.DeleteCategoryAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("DeleteFormCategory")
        .WithTags("Form Categories");
    }
}