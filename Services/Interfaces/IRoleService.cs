using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.Role;

namespace EduBridgeMVC.Services.Interfaces;

public interface IRoleService
{
    // Queries
    Task<Result<IEnumerable<RoleResponse>>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task<Result<RoleResponse>> GetRoleByIdAsync(string roleId, CancellationToken cancellationToken = default);

    // Commands
    Task<Result> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

    Task<Result> UpdateRoleAsync(string roleId, UpdateRoleRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteRoleAsync(string roleId, CancellationToken cancellationToken = default);

    // User Roles Management
    Task<Result<IEnumerable<string>>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> AssignRoleToUserAsync(AssignRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result> RemoveRoleFromUserAsync(AssignRoleRequest request, CancellationToken cancellationToken = default);
}