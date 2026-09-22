using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using BPMS.Modules.Identity.Models;
using BPMS.Modules.Identity.Services;
using BPMS.Shared.Authorization;
using BPMS.Shared.Services;

namespace BPMS.Modules.Identity.Endpoints;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles");

        group.MapGet("/", async (IRoleService service, CancellationToken ct) =>
        {
            var result = await service.ListRolesAsync(ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(Permissions.Roles.View)
        .WithName("ListRoles");

        group.MapGet("/{id:guid}", async (Guid id, IRoleService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.GetRoleAsync(id, ct);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = "Role not found." });
            }
        })
        .RequireAuthorization(Permissions.Roles.View)
        .WithName("GetRole");

        group.MapPost("/", async (CreateRoleRequest request, IRoleService service, ITenantService tenantService, CancellationToken ct) =>
        {
            // A token minted before any workspace existed carries no tenantId
            // claim; without this guard the role would silently land in an
            // empty tenant context and become invisible in every workspace.
            if (string.IsNullOrEmpty(tenantService.TenantId))
                return Results.BadRequest(new
                {
                    error = "No tenant context. Send the X-Tenant-Id header for the target workspace, or log in again so your token carries your tenant."
                });

            try
            {
                var result = await service.CreateRoleAsync(request, ct);
                return Results.Created($"/api/roles/{result.Id}", result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .RequireAuthorization(Permissions.Roles.Create)
        .WithName("CreateRole");

        group.MapPut("/{id:guid}", async (Guid id, UpdateRoleRequest request, IRoleService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.UpdateRoleAsync(id, request, ct);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = "Role not found." });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .RequireAuthorization(Permissions.Roles.Update)
        .WithName("UpdateRole");

        group.MapDelete("/{id:guid}", async (Guid id, IRoleService service, CancellationToken ct) =>
        {
            try
            {
                await service.DeleteRoleAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = "Role not found." });
            }
        })
        .RequireAuthorization(Permissions.Roles.Delete)
        .WithName("DeleteRole");

        group.MapPut("/{id:guid}/permissions", async (Guid id, SetRolePermissionsRequest request, IRoleService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.SetRolePermissionsAsync(id, request, ct);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = "Role not found." });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization(Permissions.Roles.Update)
        .WithName("SetRolePermissions");

        group.MapPost("/{id:guid}/users", async (Guid id, AssignUsersRequest request, IRoleService service, CancellationToken ct) =>
        {
            try
            {
                await service.AssignUsersAsync(id, request, ct);
                return Results.Ok(new { message = "Users assigned to role." });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = "Role not found." });
            }
        })
        .RequireAuthorization(Permissions.Roles.Update)
        .WithName("AssignUsers");

        group.MapDelete("/{id:guid}/users/{userId:guid}", async (Guid id, Guid userId, IRoleService service, CancellationToken ct) =>
        {
            try
            {
                await service.RemoveUserAsync(id, userId, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = "Role or user assignment not found." });
            }
        })
        .RequireAuthorization(Permissions.Roles.Update)
        .WithName("RemoveUser");
    }
}