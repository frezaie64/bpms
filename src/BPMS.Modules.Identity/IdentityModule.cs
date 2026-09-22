using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BPMS.Modules.Identity.Entities;
using BPMS.Modules.Identity.Models;
using BPMS.Shared.Persistence;
using BPMS.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BPMS.Modules.Identity;

public class IdentityModule : IIdentityModule
{
    private readonly BpmsDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ITenantService _tenantService;

    public IdentityModule(BpmsDbContext db, IConfiguration configuration, ITenantService tenantService)
    {
        _db = db;
        _configuration = configuration;
        _tenantService = tenantService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existingUser = await _db.Set<User>().FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (existingUser != null)
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Status = UserStatus.Registered
        };

        _db.Add(user);
        await _db.SaveChangesAsync(ct);

        return await GenerateTokens(user, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? tenantId = null, CancellationToken ct = default)
    {
        var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (user.Status == UserStatus.Suspended)
            throw new UnauthorizedAccessException("Account is suspended.");

        // Align the tenant context with the user's own tenant so the Role
        // tenant query filter matches while minting permission claims.
        _tenantService.SetTenant(user.TenantId ?? "");

        return await GenerateTokens(user, ct);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? tenantId = null, CancellationToken ct = default)
    {
        var user = await _db.Set<User>().FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, ct);
        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        // Align the tenant context with the user's own tenant so the Role
        // tenant query filter matches while minting permission claims.
        _tenantService.SetTenant(user.TenantId ?? "");

        return await GenerateTokens(user, ct);
    }

    private async Task<AuthResponse> GenerateTokens(User user, CancellationToken ct = default)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new("status", user.Status.ToString())
        };

        if (!string.IsNullOrEmpty(user.TenantId))
            claims.Add(new Claim("tenantId", user.TenantId));

        // Collect permissions from every role the user holds across all of
        // their tenant memberships. IgnoreQueryFilters bypasses the Role
        // tenant filter (which would otherwise only match the login-time
        // tenant context). Data isolation is enforced per-request by the
        // tenant query filters plus X-Tenant-Id membership validation.
        var permissions = await _db.Set<UserRole>()
            .IgnoreQueryFilters()
            .Where(ur => ur.UserId == user.Id)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync(ct);

        foreach (var permission in permissions)
            claims.Add(new System.Security.Claims.Claim("permission", permission));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        {
            KeyId = "bpms-signing-key"
        };
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = refreshExpiry;
        await _db.SaveChangesAsync(ct);

        return new AuthResponse(accessToken, refreshToken, expires);
    }
}