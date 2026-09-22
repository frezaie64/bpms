using BPMS.Modules.Identity.Models;
using BPMS.Shared.Services;

namespace BPMS.Modules.Identity.Services;

public interface IRoleService
{
    Task<List<RoleWithPermissionsResponse>> ListRolesAsync(CancellationToken ct = default);
    Task<RoleWithPermissionsResponse> GetRoleAsync(Guid id, CancellationToken ct = default);
    Task<RoleWithPermissionsResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default);
    Task<RoleWithPermissionsResponse> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken ct = default);
    Task DeleteRoleAsync(Guid id, CancellationToken ct = default);
    Task<RoleWithPermissionsResponse> SetRolePermissionsAsync(Guid id, SetRolePermissionsRequest request, CancellationToken ct = default);
    Task AssignUsersAsync(Guid id, AssignUsersRequest request, CancellationToken ct = default);
    Task RemoveUserAsync(Guid id, Guid userId, CancellationToken ct = default);
}