using BPMS.Modules.Identity.Entities;
using BPMS.Modules.Identity.Models;
using BPMS.Shared.Persistence;
using BPMS.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BPMS.Modules.Identity.Services;

public class RoleService : IRoleService
{
    private readonly BpmsDbContext _db;
    private readonly ITenantService _tenantService;

    public RoleService(BpmsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<List<RoleWithPermissionsResponse>> ListRolesAsync(CancellationToken ct = default)
    {
        var roles = await _db.Set<Role>()
            .Where(r => r.TenantId == _tenantService.TenantId)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

        return roles.Select(MapToResponse).ToList();
    }

    public async Task<RoleWithPermissionsResponse> GetRoleAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _db.Set<Role>()
            .Where(r => r.TenantId == _tenantService.TenantId)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (role is null)
            throw new KeyNotFoundException("Role not found.");

        return MapToResponse(role);
    }

    public async Task<RoleWithPermissionsResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        var existing = await _db.Set<Role>()
            .FirstOrDefaultAsync(r => r.Name == request.Name && r.TenantId == _tenantService.TenantId, ct);

        if (existing is not null)
            throw new InvalidOperationException("Role name already exists in this tenant.");

        var role = new Role
        {
            Name = request.Name,
            TenantId = _tenantService.TenantId
        };

        _db.Add(role);
        await _db.SaveChangesAsync(ct);

        return MapToResponse(role);
    }

    public async Task<RoleWithPermissionsResponse> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken ct = default)
    {
        var role = await _db.Set<Role>()
            .Where(r => r.TenantId == _tenantService.TenantId)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (role is null)
            throw new KeyNotFoundException("Role not found.");

        var duplicate = await _db.Set<Role>()
            .FirstOrDefaultAsync(r => r.Name == request.Name && r.TenantId == _tenantService.TenantId && r.Id != id, ct);

        if (duplicate is not null)
            throw new InvalidOperationException("Role name already exists in this tenant.");

        role.Name = request.Name;
        await _db.SaveChangesAsync(ct);

        return MapToResponse(role);
    }

    public async Task DeleteRoleAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _db.Set<Role>()
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == _tenantService.TenantId, ct);

        if (role is null)
            throw new KeyNotFoundException("Role not found.");

        _db.Remove(role);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<RoleWithPermissionsResponse> SetRolePermissionsAsync(Guid id, SetRolePermissionsRequest request, CancellationToken ct = default)
    {
        var role = await _db.Set<Role>()
            .Where(r => r.TenantId == _tenantService.TenantId)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (role is null)
            throw new KeyNotFoundException("Role not found.");

        var requested = request.Permissions.Distinct().ToList();

        var invalid = requested.Except(
            await _db.Set<Permission>().Select(p => p.Name).ToListAsync(ct)).ToList();
        if (invalid.Count > 0)
            throw new InvalidOperationException($"Unknown permissions: {string.Join(", ", invalid)}");

        var current = role.RolePermissions.Select(rp => rp.Permission.Name).ToHashSet();
        var permissionIds = await _db.Set<Permission>()
            .Where(p => requested.Contains(p.Name))
            .ToDictionaryAsync(p => p.Name, p => p.Id, ct);

        foreach (var removed in role.RolePermissions.Where(rp => !requested.Contains(rp.Permission.Name)).ToList())
            _db.Remove(removed);

        foreach (var name in requested.Where(n => !current.Contains(n)))
            _db.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionIds[name] });

        await _db.SaveChangesAsync(ct);

        return MapToResponse(role);
    }

    public async Task AssignUsersAsync(Guid id, AssignUsersRequest request, CancellationToken ct = default)
    {
        var role = await _db.Set<Role>()
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == _tenantService.TenantId, ct);

        if (role is null)
            throw new KeyNotFoundException("Role not found.");

        foreach (var userId in request.UserIds)
        {
            var userExists = await _db.Set<User>()
                .AnyAsync(u => u.Id == userId && u.TenantId == _tenantService.TenantId, ct);

            if (!userExists)
                continue;

            var alreadyAssigned = await _db.Set<UserRole>()
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == id, ct);

            if (!alreadyAssigned)
            {
                _db.Add(new UserRole { UserId = userId, RoleId = id });
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveUserAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var role = await _db.Set<Role>()
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == _tenantService.TenantId, ct);

        if (role is null)
            throw new KeyNotFoundException("Role not found.");

        var userRole = await _db.Set<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == id, ct);

        if (userRole is null)
            throw new KeyNotFoundException("User is not assigned to this role.");

        _db.Remove(userRole);
        await _db.SaveChangesAsync(ct);
    }

    private static RoleWithPermissionsResponse MapToResponse(Role role) =>
        new(
            role.Id,
            role.Name,
            role.IsActive,
            role.CreatedAt,
            role.UpdatedAt,
            role.RolePermissions?.Select(rp => rp.Permission.Name).ToList() ?? []
        );
}