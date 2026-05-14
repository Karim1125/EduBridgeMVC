using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.Role;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class RoleService(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager) : IRoleService
{
    public async Task<Result<IEnumerable<RoleResponse>>> GetAllRolesAsync(
        CancellationToken cancellationToken = default)
    {
        var roles = await roleManager.Roles
            .AsNoTracking()
            .Select(r => new RoleResponse(r.Id, r.Name!, r.IsDefault))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<RoleResponse>>(roles);
    }

    public async Task<Result<RoleResponse>> GetRoleByIdAsync(
        string roleId, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(roleId);

        return role is null ? Result.Failure<RoleResponse>(RoleErrors.RoleNotFound) : Result.Success(new RoleResponse(role.Id, role.Name!, role.IsDefault));
    }

    public async Task<Result> CreateRoleAsync(
        CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await roleManager.RoleExistsAsync(request.Name);

        if (exists)
            return Result.Failure(RoleErrors.RoleAlreadyExists);

        var role = new ApplicationRole
        {
            Name = request.Name,
            IsDefault = request.IsDefault
        };

        var result = await roleManager.CreateAsync(role);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(new Error("Role.CreateFailed", result.Errors.First().Description,
                StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UpdateRoleAsync(
        string roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(roleId);

        if (role is null)
            return Result.Failure(RoleErrors.RoleNotFound);

        role.Name = request.Name;
        role.IsDefault = request.IsDefault;

        var result = await roleManager.UpdateAsync(role);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(new Error("Role.UpdateFailed", result.Errors.First().Description,
                StatusCodes.Status400BadRequest));
    }

    public async Task<Result> DeleteRoleAsync(
        string roleId, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(roleId);

        if (role is null)
            return Result.Failure(RoleErrors.RoleNotFound);

        var result = await roleManager.DeleteAsync(role);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(new Error("Role.DeleteFailed", result.Errors.First().Description,
                StatusCodes.Status400BadRequest));
    }

    public async Task<Result<IEnumerable<string>>> GetUserRolesAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<IEnumerable<string>>(RoleErrors.UserNotFound);

        var roles = await userManager.GetRolesAsync(user);

        return Result.Success<IEnumerable<string>>(roles);
    }

    public async Task<Result> AssignRoleToUserAsync(
        AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(request.UserId);

        if (user is null)
            return Result.Failure(RoleErrors.UserNotFound);

        if (await userManager.IsInRoleAsync(user, request.RoleName))
            return Result.Failure(RoleErrors.UserAlreadyInRole);

        var result = await userManager.AddToRoleAsync(user, request.RoleName);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(new Error("Role.AssignFailed", result.Errors.First().Description,
                StatusCodes.Status400BadRequest));
    }

    public async Task<Result> RemoveRoleFromUserAsync(
        AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(request.UserId);

        if (user is null)
            return Result.Failure(RoleErrors.UserNotFound);

        if (!await userManager.IsInRoleAsync(user, request.RoleName))
            return Result.Failure(RoleErrors.UserNotInRole);

        var result = await userManager.RemoveFromRoleAsync(user, request.RoleName);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(new Error("Role.RemoveFailed", result.Errors.First().Description,
                StatusCodes.Status400BadRequest));
    }
}