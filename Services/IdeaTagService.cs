using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.Idea;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class IdeaTagService(
    ApplicationDbContext context,
    IMapper mapper) : IIdeaTagService
{
    public async Task<Result<IEnumerable<IdeaTagResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var tags = await context.IdeaTags
            .AsNoTracking()
            .Include(t => t.Category)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<IdeaTagResponse>>(tags));
    }

    public async Task<Result<IEnumerable<IdeaTagResponse>>> GetByCategoryAsync(
        Guid categoryId, CancellationToken cancellationToken = default)
    {
        var tags = await context.IdeaTags
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.CategoryId == categoryId)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<IdeaTagResponse>>(tags));
    }

    public async Task<Result<Guid>> GetOrCreateAsync(
        string name, Guid categoryId, CancellationToken cancellationToken = default)
    {
        name = name.Trim().ToLowerInvariant();

        var existing = await context.IdeaTags
            .FirstOrDefaultAsync(t => t.Name == name && t.CategoryId == categoryId, cancellationToken);

        if (existing is not null)
            return Result.Success(existing.Id);

        try
        {
            var tag = new IdeaTag { Name = name, CategoryId = categoryId };
            await context.IdeaTags.AddAsync(tag, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success(tag.Id);
        }
        catch (DbUpdateException)
        {
            var tag = await context.IdeaTags
                .FirstOrDefaultAsync(t => t.Name == name && t.CategoryId == categoryId, cancellationToken);

            return Result.Success(tag!.Id);
        }
    }

    public async Task<Result<IdeaTagResponse>> UpdateAsync(
        Guid id, UpdateIdeaTagRequest request, CancellationToken cancellationToken = default)
    {
        var tag = await context.IdeaTags
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tag is null)
            return Result.Failure<IdeaTagResponse>(IdeaTagErrors.TagNotFound);

        var targetCategoryId = request.CategoryId ?? tag.CategoryId;
        var normalizedName = request.Name?.Trim().ToLowerInvariant() ?? tag.Name;

        var nameExists = await context.IdeaTags
            .AnyAsync(t => t.Name == normalizedName && t.CategoryId == targetCategoryId && t.Id != id,
                cancellationToken);

        if (nameExists)
            return Result.Failure<IdeaTagResponse>(IdeaTagErrors.DuplicateTagName);

        tag.Name = normalizedName;
        tag.CategoryId = targetCategoryId;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.Map<IdeaTagResponse>(tag));
    }

    public async Task<Result> DeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await context.FindAsync<IdeaTag>([id], cancellationToken);

        if (tag is null || tag.IsDeleted)
            return Result.Failure(IdeaTagErrors.TagNotFound);

        tag.IsDeleted = true;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}