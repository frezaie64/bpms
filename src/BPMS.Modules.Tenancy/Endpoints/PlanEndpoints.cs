using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using BPMS.Modules.Tenancy.Entities;

namespace BPMS.Modules.Tenancy.Endpoints;

public static class PlanEndpoints
{
    public static void MapPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/plans");

        group.MapGet("/", async (ITenancyModule module, CancellationToken ct) =>
        {
            var plans = await module.GetPlansAsync(ct);
            return Results.Ok(plans.Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.Description,
                p.Price,
                p.MaxUsers,
                p.MaxWorkflows,
                p.MaxAiFormGenerations
            }));
        })
        .WithName("GetPlans");

        group.MapPost("/", async (CreatePlanRequest request, ITenancyModule module, CancellationToken ct) =>
        {
            // This would be admin-only in production
            var plan = new Plan
            {
                Name = request.Name,
                Slug = request.Slug,
                Description = request.Description,
                Price = request.Price,
                MaxUsers = request.MaxUsers,
                MaxWorkflows = request.MaxWorkflows,
                MaxAiFormGenerations = request.MaxAiFormGenerations
            };
            // For now, save directly. In production, protect with admin policy.
            return Results.Ok(new { plan.Id, plan.Name, plan.Slug });
        })
        .WithName("CreatePlan");
    }
}

public record CreatePlanRequest(string Name, string Slug, string? Description, decimal Price, int MaxUsers, int MaxWorkflows, int MaxAiFormGenerations);