using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using BPMS.Modules.Identity.Models;
using BPMS.Modules.Identity.Services;
using BPMS.Shared.Authorization;

namespace BPMS.Modules.Identity.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");

        group.MapGet("/me", async (IUserService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.GetProfileAsync(ct);
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .RequireAuthorization()
        .WithName("GetProfile");

        group.MapPut("/me", async (UpdateProfileRequest request, IUserService service, CancellationToken ct) =>
        {
            var result = await service.UpdateProfileAsync(request, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("UpdateProfile");

        group.MapPost("/change-password", async (ChangePasswordRequest request, IUserService service, CancellationToken ct) =>
        {
            try
            {
                await service.ChangePasswordAsync(request, ct);
                return Results.Ok(new { message = "Password changed successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization()
        .WithName("ChangePassword");

        group.MapGet("/", async (IUserService service, CancellationToken ct) =>
        {
            var result = await service.ListUsersAsync(ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(Permissions.Users.View)
        .WithName("ListUsers");

        group.MapGet("/{id:guid}", async (Guid id, IUserService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.GetUserAsync(id, ct);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = "User not found." });
            }
        })
        .RequireAuthorization(Permissions.Users.View)
        .WithName("GetUser");

        group.MapPost("/", async (CreateUserRequest request, IUserService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.CreateUserAsync(request, ct);
                return Results.Created($"/api/users/{result.Id}", result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .RequireAuthorization(Permissions.Users.Create)
        .WithName("CreateUser");

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest request, IUserService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.UpdateUserAsync(id, request, ct);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = "User not found." });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .RequireAuthorization(Permissions.Users.Update)
        .WithName("UpdateUser");

        group.MapPut("/{id:guid}/status", async (Guid id, SetUserStatusRequest request, IUserService service, CancellationToken ct) =>
        {
            try
            {
                await service.SetUserStatusAsync(id, request, ct);
                return Results.Ok(new { message = "User status updated." });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = "User not found." });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization(Permissions.Users.Update)
        .WithName("SetUserStatus");

        group.MapDelete("/{id:guid}", async (Guid id, IUserService service, CancellationToken ct) =>
        {
            try
            {
                await service.DeleteUserAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = "User not found." });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization(Permissions.Users.Delete)
        .WithName("DeleteUser");
    }
}