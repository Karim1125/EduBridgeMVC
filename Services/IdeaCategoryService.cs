using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.Idea;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class IdeaCategoryService(
    ApplicationDbContext context,
    IMapper mapper) : IIdeaCategoryService
{
    public async Task<Result<IEnumerable<IdeaCategoryResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await context.IdeaCategories
            .AsNoTracking()
            .Include(c => c.Tags)
            .ToListAsync(cancellationToken);

        return Result.Success(
            mapper.Map<IEnumerable<IdeaCategoryResponse>>(categories));
    }

    public async Task<Result<Guid>> GetOrCreateAsync(
        CreateIdeaCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim().ToLowerInvariant();

        var existing = await context.IdeaCategories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

        if (existing is not null && !existing.IsDeleted)
            return Result.Success(existing.Id);

        if (existing is not null)
        {
            existing.IsDeleted = false;
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success(existing.Id);
        }

        try
        {
            var category = new IdeaCategory { Name = name };
            await context.IdeaCategories.AddAsync(category, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success(category.Id);
        }
        catch (DbUpdateException)
        {
            var category = await context.IdeaCategories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

            if (category is null)
                return Result.Failure<Guid>(IdeaCategoryErrors.DuplicateCategoryName);

            if (category.IsDeleted)
            {
                category.IsDeleted = false;
                await context.SaveChangesAsync(cancellationToken);
            }

            return Result.Success(category.Id);
        }
    }

    public async Task<Result<IdeaCategoryResponse>> UpdateAsync(
        Guid id, UpdateIdeaCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await context.IdeaCategories
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
            return Result.Failure<IdeaCategoryResponse>(IdeaCategoryErrors.CategoryNotFound);

        var normalizedName = request.Name.Trim().ToLowerInvariant();

        var nameExists = await context.IdeaCategories
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Name == normalizedName && c.Id != id, cancellationToken);

        if (nameExists)
            return Result.Failure<IdeaCategoryResponse>(IdeaCategoryErrors.DuplicateCategoryName);

        category.Name = normalizedName;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.Map<IdeaCategoryResponse>(category));
    }

    public async Task<Result> DeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var category = await context.IdeaCategories
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null || category.IsDeleted)
            return Result.Failure(IdeaCategoryErrors.CategoryNotFound);

        var hasIdeas = await context.Ideas
            .AnyAsync(i => i.CategoryId == id, cancellationToken);

        if (hasIdeas)
            return Result.Failure(IdeaCategoryErrors.CategoryInUse);

        category.IsDeleted = true;
        foreach (var tag in category.Tags)
            tag.IsDeleted = true;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
