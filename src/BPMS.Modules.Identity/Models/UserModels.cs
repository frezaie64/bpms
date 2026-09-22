namespace BPMS.Modules.Identity.Models;

public record UserProfileResponse(Guid Id, string Email, string FirstName, string LastName, bool IsActive, DateTime CreatedAt);

public record UpdateProfileRequest(string FirstName, string LastName);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record CreateUserRequest(string Email, string Password, string FirstName, string LastName);

public record UpdateUserRequest(string Email, string FirstName, string LastName);

public record SetUserStatusRequest(bool IsActive);

public record UserResponse(Guid Id, string Email, string FirstName, string LastName, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt);