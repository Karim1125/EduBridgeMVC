using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.User;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext context,
    IMapper mapper) : IUserService
{
    public async Task<Result<UserProfileResponse>> GetCurrentUserAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .Include(u => u.Skills).ThenInclude(us => us.Skill)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user is null ? Result.Failure<UserProfileResponse>(UserErrors.UserNotFound) : Result.Success(mapper.Map<UserProfileResponse>(user));
    }

    public async Task<Result<UserResponse>> GetUserByIdAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        var roles = await userManager.GetRolesAsync(user);

        var response = mapper.Map<UserResponse>(user) with
        {
            Role = roles.FirstOrDefault() ?? string.Empty
        };

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetAllUsersAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = new List<UserResponse>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var userResponse = mapper.Map<UserResponse>(user) with
            {
                Role = roles.FirstOrDefault() ?? string.Empty
            };
            response.Add(userResponse);
        }

        return Result.Success<IEnumerable<UserResponse>>(response);
    }

    public async Task<Result<UserProfileResponse>> UpdateProfileAsync(
        string userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

        request.Adapt(user);

        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded)
            return await GetCurrentUserAsync(userId, cancellationToken);

        var error = result.Errors.First();
        return Result.Failure<UserProfileResponse>(
            new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

    }

    public async Task<Result> UploadProfileImageAsync(
        string userId, IFormFile image, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        if (image.Length == 0)
            return Result.Failure(UserErrors.InvalidImage);

        var uploadsFolder = Path.Combine("wwwroot", "images", "profiles");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await image.CopyToAsync(stream, cancellationToken);

        user.ProfileImageUrl = $"/images/profiles/{fileName}";

        await userManager.UpdateAsync(user);

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(
        string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var result = await userManager.ChangePasswordAsync(
            user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure(
                new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        return Result.Success();
    }

    public async Task<Result<IEnumerable<SkillResponse>>> GetUserSkillsAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .Include(u => u.Skills).ThenInclude(us => us.Skill)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return Result.Failure<IEnumerable<SkillResponse>>(UserErrors.UserNotFound);

        var skills = user.Skills.Select(us => new SkillResponse(us.Skill.Id, us.Skill.Name));

        return Result.Success(skills);
    }

    public async Task<Result> AddSkillsAsync(
        string userId, List<string> skillNames, CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .Include(u => u.Skills)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        foreach (var normalized in skillNames.Select(skillName => skillName.Trim().ToLowerInvariant()))
        {
            var skill = await context.Skills
                .FirstOrDefaultAsync(s => s.Name == normalized, cancellationToken);

            if (skill is null)
            {
                skill = new Skill { Name = normalized };
                await context.Skills.AddAsync(skill, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }

            if (user.Skills.All(us => us.SkillId != skill.Id))
                user.Skills.Add(new UserSkill { UserId = userId, SkillId = skill.Id });
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveSkillAsync(
        string userId, Guid skillId, CancellationToken cancellationToken = default)
    {
        var userSkill = await context.UserSkills
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId, cancellationToken);

        if (userSkill is null)
            return Result.Failure(UserErrors.SkillNotFound);

        context.UserSkills.Remove(userSkill);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ClearSkillsAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        await context.UserSkills
            .Where(us => us.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        // Soft-delete: just mark as disabled so they cannot login
        user.IsDisabled = true;

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        return Result.Success();
    }

    public async Task<Result<bool>> ToggleDisableAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<bool>(UserErrors.UserNotFound);

        user.IsDisabled = !user.IsDisabled;

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure<bool>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        return Result.Success<bool>(user.IsDisabled);
    }

    public async Task<Result> AddAsync(
        CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
            return Result.Failure(UserErrors.DuplicatedEmail);

        var user = request.Adapt<ApplicationUser>();
        user.UserName = request.Email;

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure(
                new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        await userManager.AddToRoleAsync(user, request.Role);

        return Result.Success();
    }
}