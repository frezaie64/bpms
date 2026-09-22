using BPMS.Modules.Identity.Entities;
using BPMS.Modules.Identity.Models;
using BPMS.Shared.Persistence;
using BPMS.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BPMS.Modules.Identity.Services;

public class UserService : IUserService
{
    private readonly BpmsDbContext _db;
    private readonly ITenantService _tenantService;
    private readonly IPasswordPolicy _passwordPolicy;

    public UserService(BpmsDbContext db, ITenantService tenantService, IPasswordPolicy passwordPolicy)
    {
        _db = db;
        _tenantService = tenantService;
        _passwordPolicy = passwordPolicy;
    }

    public async Task<UserProfileResponse> GetProfileAsync(CancellationToken ct = default)
    {
        var user = await GetCurrentUserAsync(ct);
        return MapToProfile(user);
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await GetCurrentUserAsync(ct);
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        await _db.SaveChangesAsync(ct);
        return MapToProfile(user);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await GetCurrentUserAsync(ct);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        _passwordPolicy.Validate(request.NewPassword);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<UserResponse>> ListUsersAsync(CancellationToken ct = default)
    {
        var users = await _db.Set<User>()
            .Where(u => u.TenantId == _tenantService.TenantId)
            .OrderBy(u => u.Email)
            .ToListAsync(ct);

        return users.Select(MapToResponse).ToList();
    }

    public async Task<UserResponse> GetUserAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == _tenantService.TenantId, ct);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        return MapToResponse(user);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        _passwordPolicy.Validate(request.Password);

        var existing = await _db.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.TenantId == _tenantService.TenantId, ct);

        if (existing is not null)
            throw new InvalidOperationException("Email already registered in this tenant.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            TenantId = _tenantService.TenantId
        };

        _db.Add(user);
        await _db.SaveChangesAsync(ct);

        return MapToResponse(user);
    }

    public async Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _db.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == _tenantService.TenantId, ct);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        var duplicate = await _db.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.TenantId == _tenantService.TenantId && u.Id != id, ct);

        if (duplicate is not null)
            throw new InvalidOperationException("Email already in use by another user in this tenant.");

        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        await _db.SaveChangesAsync(ct);

        return MapToResponse(user);
    }

    public async Task SetUserStatusAsync(Guid id, SetUserStatusRequest request, CancellationToken ct = default)
    {
        var user = await _db.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == _tenantService.TenantId, ct);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        if (!request.IsActive)
        {
            var activeCount = await _db.Set<User>()
                .CountAsync(u => u.TenantId == _tenantService.TenantId && u.Status == UserStatus.Active && u.Id != id, ct);

            if (activeCount == 0)
                throw new InvalidOperationException("Cannot deactivate the last active user in the tenant.");
        }

        user.Status = request.IsActive ? UserStatus.Active : UserStatus.Suspended;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == _tenantService.TenantId, ct);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        var activeCount = await _db.Set<User>()
            .CountAsync(u => u.TenantId == _tenantService.TenantId && u.Status == UserStatus.Active && u.Id != id, ct);

        if (activeCount == 0)
            throw new InvalidOperationException("Cannot delete the last active user in the tenant.");

        user.IsDeleted = true;
        user.Status = UserStatus.Suspended;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_tenantService.UserId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var userId = Guid.Parse(_tenantService.UserId);
        var user = await _db.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            throw new UnauthorizedAccessException("User not found.");

        return user;
    }

    private static UserProfileResponse MapToProfile(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.Status == UserStatus.Active, user.CreatedAt);

    private static UserResponse MapToResponse(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.Status == UserStatus.Active, user.CreatedAt, user.UpdatedAt);
}