using System.Security.Claims;
using BPMS.Modules.Tenancy.Entities;
using BPMS.Modules.Tenancy.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.Tenancy.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants");

        // Create tenant (requires active subscription)
        group.MapPost("/", async (CreateTenantRequest request, ITenancyModule module, HttpContext context, CancellationToken ct) =>
        {
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            try
            {
                var tenant = await module.CreateTenantAsync(request.Name, request.Slug, userId, ct);
                return Results.Created($"/api/tenants/{tenant.Id}", new
                {
                    tenant.Id,
                    tenant.Name,
                    tenant.Slug,
                    tenant.Status
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateTenant")
        .RequireAuthorization();

        // List all tenants
        group.MapGet("/", async (ITenancyModule module, CancellationToken ct) =>
        {
            var tenants = await module.GetTenantsAsync(ct);
            return Results.Ok(tenants.Select(t => new
            {
                t.Id,
                t.Name,
                t.Slug,
                t.Status
            }));
        })
        .WithName("GetTenants")
        .RequireAuthorization();

        // Get current user's tenants
        group.MapGet("/my", async (ITenancyModule module, HttpContext context, CancellationToken ct) =>
        {
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var tenants = await module.GetUserTenantsAsync(userId, ct);
            return Results.Ok(tenants.Select(t => new
            {
                t.Id,
                t.Name,
                t.Slug,
                t.Status
            }));
        })
        .WithName("GetMyTenants")
        .RequireAuthorization();

        // Get tenant by ID
        group.MapGet("/{id}", async (Guid id, ITenancyModule module, CancellationToken ct) =>
        {
            var tenant = await module.GetTenantByIdAsync(id, ct);
            if (tenant is null)
                return Results.NotFound();
            return Results.Ok(new
            {
                tenant.Id,
                tenant.Name,
                tenant.Slug,
                tenant.Status
            });
        })
        .WithName("GetTenantById")
        .RequireAuthorization();

        // Add member by UserId (backward compatible)
        group.MapPost("/{id}/invite", async (Guid id, AddMemberRequest request, ITenancyModule module, CancellationToken ct) =>
        {
            try
            {
                var member = await module.AddMemberAsync(id, request.UserId, request.Role, ct);
                return Results.Ok(new { member.TenantId, member.UserId, member.Role });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("InviteMember")
        .RequireAuthorization();

        // Send email invitation
        group.MapPost("/{id}/invite-by-email", async (Guid id, InviteByEmailRequest request, ITenancyModule module, HttpContext context, CancellationToken ct) =>
        {
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            try
            {
                var invitation = await module.SendInvitationAsync(id, request.Email, request.Role, userId, ct);
                return Results.Ok(new InvitationResponse(
                    invitation.Id,
                    invitation.Email,
                    invitation.Role,
                    invitation.Status,
                    invitation.ExpiresAt,
                    invitation.AcceptedAt,
                    invitation.CreatedAt
                ));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("InviteByEmail")
        .RequireAuthorization();

        // Get tenant members
        group.MapGet("/{id}/members", async (Guid id, ITenancyModule module, CancellationToken ct) =>
        {
            var members = await module.GetMembersAsync(id, ct);
            return Results.Ok(members.Select(m => new
            {
                m.TenantId,
                m.UserId,
                m.Role,
                m.JoinedAt
            }));
        })
        .WithName("GetTenantMembers")
        .RequireAuthorization();

        // Get tenant invitations
        group.MapGet("/{id}/invitations", async (Guid id, ITenancyModule module, CancellationToken ct) =>
        {
            var invitations = await module.GetTenantInvitationsAsync(id, ct);
            return Results.Ok(invitations.Select(i => new InvitationResponse(
                i.Id, i.Email, i.Role, i.Status, i.ExpiresAt, i.AcceptedAt, i.CreatedAt
            )));
        })
        .WithName("GetTenantInvitations")
        .RequireAuthorization();
    }
}

public record CreateTenantRequest(string Name, string Slug);
public record AddMemberRequest(Guid UserId, TenantMemberRole Role = TenantMemberRole.Member);
