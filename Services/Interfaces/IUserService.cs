using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.User;

namespace EduBridgeMVC.Services.Interfaces;

public interface IUserService
{
    // Queries
    Task<Result<UserProfileResponse>> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserResponse>>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    // Profile operationsؤ
    Task<Result<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> UploadProfileImageAsync(string userId, IFormFile image, CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request,
        CancellationToken cancellationToken = default);

    // Skill operations
    Task<Result<IEnumerable<SkillResponse>>> GetUserSkillsAsync(string userId,
        CancellationToken cancellationToken = default);

    Task<Result> AddSkillsAsync(string userId, List<string> skillNames, CancellationToken cancellationToken = default);
    Task<Result> RemoveSkillAsync(string userId, Guid skillId, CancellationToken cancellationToken = default);
    Task<Result> ClearSkillsAsync(string userId, CancellationToken cancellationToken = default);

    // Admin operations
    Task<Result> AddAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<bool>> ToggleDisableAsync(string userId, CancellationToken cancellationToken = default);
}