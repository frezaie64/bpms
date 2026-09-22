using System.Security.Claims;
using BPMS.Modules.WorkflowRuntime.Models;
using BPMS.Shared.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BPMS.Modules.WorkflowRuntime.Endpoints;

public static class AdminDashboardEndpoints
{
    public static void MapAdminDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin").RequireAuthorization();

        group.MapGet("/dashboard", async (IWorkflowRuntimeModule module, CancellationToken ct) =>
            Results.Ok(await module.GetAdminDashboardAsync(ct)))
            .RequireAuthorization(Permissions.Administration.ViewDashboard)
            .WithName("GetAdminDashboard")
            .WithTags("Administration");
    }
}