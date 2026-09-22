using System.Security.Claims;
using BPMS.Modules.Tenancy.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.Tenancy.Endpoints;

public static class InvitationEndpoints
{
    public static void MapInvitationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invitations");

        // Accept an invitation by token
        group.MapPost("/accept", async (AcceptInvitationRequest request, ITenancyModule module, HttpContext context, CancellationToken ct) =>
        {
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            try
            {
                var member = await module.AcceptInvitationAsync(request.Token, userId, ct);
                return Results.Ok(new InvitationAcceptResponse(
                    member.TenantId,
                    member.UserId,
                    member.Role
                ));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("AcceptInvitation")
        .RequireAuthorization();
    }
}
