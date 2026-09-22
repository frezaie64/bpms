using BPMS.Modules.FormGenerator.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.FormGenerator.Endpoints;

public static class FormSubmissionEndpoints
{
    public static void MapFormSubmissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/form-generator/{formId:guid}/submissions");

        // Allow anonymous access for public form submissions
        group.MapPost("/", async (
            Guid formId,
            SubmitFormRequest request,
            IFormGeneratorModule module,
            CancellationToken ct) =>
        {
            if (request.Data is null)
                return Results.BadRequest(new { error = "data is required" });

            try
            {
                var submission = await module.SubmitFormAsync(formId, request.SubmittedBy, request.Data, ct);
                return Results.Created($"/api/form-generator/{formId}/submissions/{submission.Id}", submission);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
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
        .AllowAnonymous()
        .WithName("SubmitGeneratedForm")
        .WithTags("Form Submissions");

        group.MapGet("/{submissionId:guid}", async (
            Guid formId,
            Guid submissionId,
            IFormGeneratorModule module,
            CancellationToken ct) =>
        {
            var submission = await module.GetSubmissionAsync(formId, submissionId, ct);
            return submission is null
                ? Results.NotFound(new { error = "submission not found" })
                : Results.Ok(submission);
        })
        .WithName("GetGeneratedFormSubmission")
        .WithTags("Form Submissions");
    }
}