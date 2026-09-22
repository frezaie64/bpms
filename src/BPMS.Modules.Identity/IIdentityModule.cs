using BPMS.Modules.Identity.Models;

namespace BPMS.Modules.Identity;

public interface IIdentityModule
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string? tenantId = null, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? tenantId = null, CancellationToken ct = default);
}