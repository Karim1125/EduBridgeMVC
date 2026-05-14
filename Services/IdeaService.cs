using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.Idea;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class IdeaService(
    ApplicationDbContext context,
    IIdeaTagService tagService,
    IMapper mapper) : IIdeaService
{
    public async Task<Result<IdeaResponse>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var idea = await context.Ideas
            .AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Tags)
            .Include(i => i.Team)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        return idea is null
            ? Result.Failure<IdeaResponse>(IdeaErrors.IdeaNotFound)
            : Result.Success(mapper.Map<IdeaResponse>(idea));
    }

    public async Task<Result<IEnumerable<IdeaResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var ideas = await context.Ideas
            .AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Tags)
            .Include(i => i.Team)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<IdeaResponse>>(ideas));
    }

    public async Task<Result<IEnumerable<IdeaResponse>>> GetByCategoryAsync(
        Guid categoryId, CancellationToken cancellationToken = default)
    {
        var ideas = await context.Ideas
            .AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Tags)
            .Include(i => i.Team)
            .Where(i => i.CategoryId == categoryId)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<IdeaResponse>>(ideas));
    }

    public async Task<Result<IEnumerable<IdeaResponse>>> GetByTagAsync(
        Guid tagId, CancellationToken cancellationToken = default)
    {
        var ideas = await context.Ideas
            .AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Tags)
            .Include(i => i.Team)
            .Where(i => i.Tags.Any(t => t.Id == tagId))
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<IdeaResponse>>(ideas));
    }

    public async Task<Result<IdeaResponse>> CreateAsync(
        Guid teamId, CreateIdeaRequest request, CancellationToken cancellationToken = default)
    {
        var teamHasIdea = await context.Ideas
            .AnyAsync(i => i.TeamId == teamId, cancellationToken);

        if (teamHasIdea)
            return Result.Failure<IdeaResponse>(IdeaErrors.TeamAlreadyHasIdea);

        var tagIds = new List<Guid>();
        foreach (var tagName in request.Tags)
        {
            var tagResult = await tagService.GetOrCreateAsync(tagName, request.CategoryId, cancellationToken);
            if (tagResult.IsFailure)
                return Result.Failure<IdeaResponse>(tagResult.Error);
            tagIds.Add(tagResult.Value);
        }

        var tags = await context.IdeaTags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        var idea = new Idea
        {
            Title = request.Title,
            Description = request.Description,
            RepositoryUrl = request.RepositoryUrl,
            TeamId = teamId,
            CategoryId = request.CategoryId,
            Tags = tags
        };

        await context.Ideas.AddAsync(idea, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await context.Entry(idea).Reference(i => i.Category).LoadAsync(cancellationToken);
        await context.Entry(idea).Reference(i => i.Team).LoadAsync(cancellationToken);

        return Result.Success(mapper.Map<IdeaResponse>(idea));
    }

    public async Task<Result<IdeaResponse>> UpdateAsync(
        Guid id, UpdateIdeaRequest request, CancellationToken cancellationToken = default)
    {
        var idea = await context.Ideas
            .Include(i => i.Category)
            .Include(i => i.Tags)
            .Include(i => i.Team)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (idea is null)
            return Result.Failure<IdeaResponse>(IdeaErrors.IdeaNotFound);

        if (request.Title is not null) idea.Title = request.Title;
        if (request.Description is not null) idea.Description = request.Description;
        if (request.RepositoryUrl is not null) idea.RepositoryUrl = request.RepositoryUrl;
        if (request.CategoryId is not null) idea.CategoryId = request.CategoryId.Value;

        if (request.Tags is not null)
        {
            var categoryId = request.CategoryId ?? idea.CategoryId;
            var tagIds = new List<Guid>();

            foreach (var tagName in request.Tags)
            {
                var tagResult = await tagService.GetOrCreateAsync(tagName, categoryId, cancellationToken);
                if (tagResult.IsFailure)
                    return Result.Failure<IdeaResponse>(tagResult.Error);
                tagIds.Add(tagResult.Value);
            }

            idea.Tags = await context.IdeaTags
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.Map<IdeaResponse>(idea));
    }

    public async Task<Result> DeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var idea = await context.FindAsync<Idea>([id], cancellationToken);

        if (idea is null || idea.IsDeleted)
            return Result.Failure(IdeaErrors.IdeaNotFound);

        idea.IsDeleted = true;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}