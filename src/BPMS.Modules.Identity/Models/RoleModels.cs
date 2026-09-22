namespace BPMS.Modules.Identity.Models;

public record CreateRoleRequest(string Name);

public record UpdateRoleRequest(string Name);

public record RoleResponse(Guid Id, string Name, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt);

public record RoleWithPermissionsResponse(Guid Id, string Name, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt, List<string> Permissions);

public record AssignUsersRequest(List<Guid> UserIds);

public record SetRolePermissionsRequest(List<string> Permissions);