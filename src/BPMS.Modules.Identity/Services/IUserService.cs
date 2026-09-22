using BPMS.Modules.Identity.Models;
using BPMS.Shared.Services;

namespace BPMS.Modules.Identity.Services;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(CancellationToken ct = default);
    Task<UserProfileResponse> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken ct = default);
    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default);
    Task<List<UserResponse>> ListUsersAsync(CancellationToken ct = default);
    Task<UserResponse> GetUserAsync(Guid id, CancellationToken ct = default);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task SetUserStatusAsync(Guid id, SetUserStatusRequest request, CancellationToken ct = default);
    Task DeleteUserAsync(Guid id, CancellationToken ct = default);
}