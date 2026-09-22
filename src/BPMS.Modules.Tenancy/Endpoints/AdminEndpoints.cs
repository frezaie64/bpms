using BPMS.Modules.Tenancy.Models;
using BPMS.Shared.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.Tenancy.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var settings = app.MapGroup("/api/settings").RequireAuthorization();
        var lookups = app.MapGroup("/api/lookups").RequireAuthorization();

        settings.MapGet("/", async (ITenancyModule module, CancellationToken ct) =>
            Results.Ok(await module.GetSettingsAsync(ct)))
            .RequireAuthorization(Permissions.Administration.ManageSettings)
            .WithName("GetSettings")
            .WithTags("Settings");

        settings.MapPut("/", async (UpdateTenantSettingRequest request, ITenancyModule module, CancellationToken ct) =>
            Results.Ok(await module.UpdateSettingsAsync(request, ct)))
            .RequireAuthorization(Permissions.Administration.ManageSettings)
            .WithName("UpdateSettings")
            .WithTags("Settings");

        lookups.MapGet("/", async (string? group, ITenancyModule module, CancellationToken ct) =>
            Results.Ok(await module.GetLookupsAsync(group, ct)))
            .RequireAuthorization(Permissions.Administration.ManageLookups)
            .WithName("GetLookups")
            .WithTags("Lookups");

        lookups.MapPost("/", async (CreateLookupRequest request, ITenancyModule module, CancellationToken ct) =>
        {
            try
            {
                var result = await module.CreateLookupAsync(request, ct);
                return Results.Created($"/api/lookups/{result.Id}", result);
            }
            catch (Exception)
            {
                return Results.Problem("Failed to create lookup value.");
            }
        })
        .RequireAuthorization(Permissions.Administration.ManageLookups)
        .WithName("CreateLookup")
        .WithTags("Lookups");

        lookups.MapPut("/{id:guid}", async (Guid id, UpdateLookupRequest request, ITenancyModule module, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await module.UpdateLookupAsync(id, request, ct));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .RequireAuthorization(Permissions.Administration.ManageLookups)
        .WithName("UpdateLookup")
        .WithTags("Lookups");

        lookups.MapDelete("/{id:guid}", async (Guid id, ITenancyModule module, CancellationToken ct) =>
        {
            try
            {
                await module.DeleteLookupAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .RequireAuthorization(Permissions.Administration.ManageLookups)
        .WithName("DeleteLookup")
        .WithTags("Lookups");
    }
}