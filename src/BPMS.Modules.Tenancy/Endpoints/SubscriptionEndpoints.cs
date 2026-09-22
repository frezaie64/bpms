using System.Security.Claims;
using BPMS.Modules.Tenancy.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.Tenancy.Endpoints;

public static class SubscriptionEndpoints
{
    public static void MapSubscriptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/subscriptions");

        // Create subscription (also creates a pending payment)
        group.MapPost("/", async (CreateSubscriptionRequest request, ITenancyModule module, HttpContext context, CancellationToken ct) =>
        {
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            try
            {
                var subscription = await module.CreateSubscriptionAsync(userId, request.PlanId, ct);
                return Results.Ok(new SubscriptionResponse(
                    subscription.Id,
                    subscription.UserId,
                    subscription.PlanId,
                    subscription.Status,
                    subscription.StartDate,
                    subscription.EndDate
                ));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateSubscription")
        .RequireAuthorization();

        // Get current user's subscriptions
        group.MapGet("/my", async (ITenancyModule module, HttpContext context, CancellationToken ct) =>
        {
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var subscriptions = await module.GetUserSubscriptionsAsync(userId, ct);
            return Results.Ok(subscriptions.Select(s => new SubscriptionResponse(
                s.Id, s.UserId, s.PlanId, s.Status, s.StartDate, s.EndDate
            )));
        })
        .WithName("GetMySubscriptions")
        .RequireAuthorization();

        // Get current user's active subscription status
        group.MapGet("/my/status", async (ITenancyModule module, HttpContext context, CancellationToken ct) =>
        {
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var subscription = await module.GetActiveSubscriptionForUserAsync(userId, ct);
            if (subscription is null)
                return Results.Ok(new SubscriptionStatusResponse(false, null, null, null, null));

            return Results.Ok(new SubscriptionStatusResponse(
                true,
                subscription.Id,
                subscription.PlanId,
                null, // plan name resolved separately if needed
                subscription.EndDate
            ));
        })
        .WithName("GetMySubscriptionStatus")
        .RequireAuthorization();

        // Simulate payment for a subscription
        group.MapPost("/{id}/pay", async (Guid id, ITenancyModule module, HttpContext context, CancellationToken ct) =>
        {
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            try
            {
                var payment = await module.SimulatePaymentAsync(id, userId, ct);
                return Results.Ok(new PayPaymentResponse(
                    payment.Id,
                    payment.Status,
                    payment.TransactionId,
                    payment.Amount
                ));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("SimulatePayment")
        .RequireAuthorization();
    }
}
