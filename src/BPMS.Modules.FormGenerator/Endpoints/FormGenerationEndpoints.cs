using System.Security.Claims;
using BPMS.Modules.FormGenerator.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.FormGenerator.Endpoints;

public static class FormGenerationEndpoints
{
    public static void MapFormGenerationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/form-generator").RequireAuthorization();

        group.MapPost("/generate", async (
            GenerateFormRequest request,
            IFormGeneratorModule module,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return Results.BadRequest(new { error = "prompt is required" });

            try
            {
                var result = await module.GenerateFormDraftAsync(request.Prompt, ct);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        })
        .WithName("GenerateForm")
        .WithTags("Form Generator");

        group.MapPost("/accept", async (
            AcceptFormRequest request,
            IFormGeneratorModule module,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            if (request.Form is null)
                return Results.BadRequest(new { error = "form is required" });

            try
            {
                var userId = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                var result = await module.AcceptAndSaveFormAsync(request.Form, userId, ct);
                return Results.Created($"/api/form-generator/{result.Id}", result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        })
        .WithName("AcceptGeneratedForm")
        .WithTags("Form Generator");

        group.MapGet("/{formId:guid}", async (
            Guid formId,
            IFormGeneratorModule module,
            CancellationToken ct) =>
        {
            var form = await module.GetFormAsync(formId, ct);
            return form is null ? Results.NotFound(new { error = "form not found" }) : Results.Ok(form);
        })
        .WithName("GetGeneratedForm")
        .WithTags("Form Generator");

        group.MapPost("/bind", async (
            BindFormRequest request,
            IFormGeneratorModule module,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.TaskId) || request.FormId == Guid.Empty)
                return Results.BadRequest(new { error = "task_id and form_id are required" });

            try
            {
                await module.BindFormToTaskAsync(request.TaskId, request.FormId, ct);
                return Results.Ok(new BindFormResponse());
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        })
        .WithName("BindFormToTask")
        .WithTags("Form Generator");

        group.MapGet("/for-task/{taskId}", async (
            string taskId,
            IFormGeneratorModule module,
            CancellationToken ct) =>
        {
            var formId = await module.GetFormIdForTaskAsync(taskId, ct);
            return formId is null
                ? Results.NotFound(new { error = "no binding found for this task" })
                : Results.Ok(new { form_id = formId });
        })
        .WithName("GetFormForTask")
        .WithTags("Form Generator");
    }
}