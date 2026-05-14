using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.Skills;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class SkillService(ApplicationDbContext context) : ISkillService
{
    public async Task<Result<IEnumerable<SkillResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var skills = await context.Skills
            .AsNoTracking()
            .Select(s => new SkillResponse(s.Id, s.Name))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<SkillResponse>>(skills);
    }

    public async Task<Result<Guid>> GetOrCreateAsync(
        string skillName, CancellationToken cancellationToken = default)
    {
        skillName = skillName.Trim().ToLowerInvariant();

        var existing = await context.Skills
            .FirstOrDefaultAsync(s => s.Name == skillName, cancellationToken);

        if (existing is not null)
            return Result.Success(existing.Id);

        try
        {
            var skill = new Skill { Name = skillName };
            await context.Skills.AddAsync(skill, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success(skill.Id);
        }

        catch (DbUpdateException)
        {
            var skill = await context.Skills
                .FirstOrDefaultAsync(s => s.Name == skillName, cancellationToken);

            return Result.Success(skill!.Id);
        }
    }

    public async Task<Result<SkillResponse>> UpdateAsync(
        Guid id, UpdateSkillRequest request, CancellationToken cancellationToken = default)
    {
        var skill = await context.FindAsync<Skill>([id], cancellationToken);

        if (skill is null || skill.IsDeleted)
            return Result.Failure<SkillResponse>(SkillErrors.SkillNotFound);

        var normalizedName = request.Name.Trim().ToLowerInvariant();

        var nameExists = await context.Skills
            .AnyAsync(s => s.Name == normalizedName && s.Id != id, cancellationToken);

        if (nameExists)
            return Result.Failure<SkillResponse>(SkillErrors.DuplicateSkillName);

        skill.Name = normalizedName;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new SkillResponse(skill.Id, skill.Name));
    }

    public async Task<Result> DeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var skill = await context.FindAsync<Skill>([id], cancellationToken);

        if (skill is null || skill.IsDeleted)
            return Result.Failure(SkillErrors.SkillNotFound);

        skill.IsDeleted = true;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}