using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Contracts.Rating;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class RatingService(
    ApplicationDbContext context,
    INotificationService notificationService,
    IMapper mapper) : IRatingService
{
    public async Task<Result<IEnumerable<RatingResponse>>> GetByTaAsync(
        Guid taId, CancellationToken cancellationToken = default)
    {
        var ratings = await context.Ratings
            .AsNoTracking()
            .Include(r => r.Team)
            .Include(r => r.Ta).ThenInclude(ta => ta.User)
            .Where(r => r.TaId == taId)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<RatingResponse>>(ratings));
    }

    public async Task<Result<RatingResponse>> GetByTeamAsync(
        Guid teamId, CancellationToken cancellationToken = default)
    {
        var rating = await context.Ratings
            .AsNoTracking()
            .Include(r => r.Team)
            .Include(r => r.Ta).ThenInclude(ta => ta.User)
            .FirstOrDefaultAsync(r => r.TeamId == teamId, cancellationToken);

        if (rating is null)
            return Result.Failure<RatingResponse>(RatingErrors.RatingNotFound);

        return Result.Success(mapper.Map<RatingResponse>(rating));
    }

    public async Task<Result<double>> GetAverageAsync(
        Guid taId, CancellationToken cancellationToken = default)
    {
        var ratings = await context.Ratings
            .Where(r => r.TaId == taId)
            .ToListAsync(cancellationToken);

        var average = ratings.Count == 0 ? 0.0 : ratings.Average(r => r.Score);

        return Result.Success(average);
    }

    public async Task<Result> SubmitAsync(
        Guid teamId, SubmitRatingRequest request, CancellationToken cancellationToken = default)
    {
        var team = await context.Teams
            .FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);

        if (team is null)
            return Result.Failure(RatingErrors.RatingNotFound);

        if (team.TaId is null)
            return Result.Failure(RatingErrors.TeamHasNoTa);

        var alreadyRated = await context.Ratings
            .AnyAsync(r => r.TeamId == teamId, cancellationToken);

        if (alreadyRated)
            return Result.Failure(RatingErrors.TeamAlreadyRated);

        var rating = new Rating
        {
            TeamId = teamId,
            TaId = team.TaId.Value,
            Score = request.Score,
            Feedback = request.Feedback
        };

        await context.Ratings.AddAsync(rating, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var ta = await context.TeachingAssistants
            .FirstOrDefaultAsync(t => t.Id == team.TaId.Value, cancellationToken);

        if (ta is not null)
            await notificationService.SendAsync(
                ta.UserId,
                NotificationType.RatingReceived,
                $"You received a new rating of {request.Score}/5.",
                rating.Id,
                cancellationToken);

        return Result.Success();
    }
}