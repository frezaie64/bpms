using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using BPMS.Modules.Identity.Models;

namespace BPMS.Modules.Identity.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (RegisterRequest request, IIdentityModule module, CancellationToken ct) =>
        {
            try
            {
                var result = await module.RegisterAsync(request, ct);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .WithName("Register");

        group.MapPost("/login", async (LoginRequest request, IIdentityModule module, CancellationToken ct) =>
        {
            try
            {
                var result = await module.LoginAsync(request, null, ct);
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .WithName("Login");

        group.MapPost("/refresh", async (RefreshTokenRequest request, IIdentityModule module, CancellationToken ct) =>
        {
            try
            {
                var result = await module.RefreshTokenAsync(request, null, ct);
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .WithName("RefreshToken");
    }
}