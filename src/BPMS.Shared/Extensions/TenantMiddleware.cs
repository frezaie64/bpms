using BPMS.Shared.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BPMS.Shared.Extensions;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService, ITenantAccessService tenantAccess)
    {
        var tenantHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(tenantHeader))
        {
            // An authenticated user may only select a tenant they belong to
            // (or be a global admin) — prevents cross-tenant spoofing.
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User?.FindFirst("sub")?.Value
                               ?? context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(tenantHeader, out var tenantId))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = "Invalid X-Tenant-Id header." });
                    return;
                }

                if (!Guid.TryParse(userIdClaim, out var userId)
                    || (!await tenantAccess.IsMemberAsync(tenantId, userId)
                        && !await tenantAccess.IsGlobalAdminAsync(userId)))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { error = "You are not a member of this tenant." });
                    return;
                }
            }

            tenantService.SetTenant(tenantHeader);
        }
        else
        {
            var tenantClaim = context.User?.FindFirst("tenantId")?.Value;
            if (!string.IsNullOrEmpty(tenantClaim))
            {
                tenantService.SetTenant(tenantClaim);
            }
        }

        // Try "sub" (JWT standard) first, fall back to .NET mapped claim type
        var userIdClaimForContext = context.User?.FindFirst("sub")?.Value
                                 ?? context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userIdClaimForContext))
        {
            tenantService.SetUser(userIdClaimForContext);
        }

        await _next(context);
    }
}

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}
