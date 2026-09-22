using BPMS.Modules.FileManagement.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.FileManagement.Endpoints;

public static class FileEndpoints
{
    public static void MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/files").RequireAuthorization();

        group.MapGet("/", async (IFileManagementModule module, string? category, CancellationToken ct) =>
            Results.Ok(await module.GetFilesAsync(category, ct)))
            .WithName("GetFiles")
            .WithTags("Files");

        group.MapGet("/{id:guid}", async (Guid id, IFileManagementModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.GetFileByIdAsync(id, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("GetFileById")
        .WithTags("Files");

        group.MapPost("/upload", async (IFormFile file, string? category, IFileManagementModule module, CancellationToken ct) =>
        {
            try
            {
                var result = await module.UploadFileAsync(file, category ?? "general", ct);
                return Results.Created($"/api/files/{result.Id}", result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("UploadFile")
        .WithTags("Files")
        .DisableAntiforgery();

        group.MapGet("/{id:guid}/download", async (Guid id, IFileManagementModule module, CancellationToken ct) =>
        {
            try
            {
                var (content, contentType, fileName) = await module.DownloadFileAsync(id, ct);
                return Results.File(content, contentType, fileName);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("DownloadFile")
        .WithTags("Files");

        group.MapGet("/{id:guid}/preview", async (Guid id, IFileManagementModule module, CancellationToken ct) =>
        {
            try
            {
                var (content, contentType, fileName) = await module.DownloadFileAsync(id, ct);
                return Results.File(content, contentType);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("PreviewFile")
        .WithTags("Files");

        group.MapPut("/{id:guid}", async (Guid id, RenameFileRequest request, IFileManagementModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.RenameFileAsync(id, request, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("RenameFile")
        .WithTags("Files");

        group.MapDelete("/{id:guid}", async (Guid id, IFileManagementModule module, CancellationToken ct) =>
        {
            try
            {
                await module.DeleteFileAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("DeleteFile")
        .WithTags("Files");
    }
}