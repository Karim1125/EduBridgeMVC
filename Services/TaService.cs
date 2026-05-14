using System.Security.Claims;
using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.TA;
using EduBridgeMVC.Contracts.Team;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class TaService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IRatingService ratingService,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper) : ITaService
{
    private string? CurrentUserId =>
        httpContextAccessor.HttpContext?.User?
        .FindFirstValue(ClaimTypes.NameIdentifier);

    public async Task<Result<IEnumerable<TAResponse>>> GetAllTAsAsync(
        CancellationToken cancellationToken = default)
    {
        var tas = await context.TeachingAssistants
            .AsNoTracking()
            .Include(ta => ta.User)
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

        return Result.Success(await MapWithRatingsAsync(tas, cancellationToken));
    }

    public async Task<Result<TAResponse>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var ta = await context.TeachingAssistants
            .AsNoTracking()
            .Include(ta => ta.User)
            .FirstOrDefaultAsync(ta => ta.Id == id && !ta.IsDeleted, cancellationToken);

        if (ta is null)
            return Result.Failure<TAResponse>(TaErrors.TaNotFound);

        var avgResult = await ratingService.GetAverageAsync(ta.Id, cancellationToken);

        var response = mapper.Map<TAResponse>(ta) with
        {
            AverageRating = avgResult.IsSuccess ? avgResult.Value : 0
        };

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<TAResponse>>> GetAvailableTAsAsync(
        CancellationToken cancellationToken = default)
    {
        var tas = await context.TeachingAssistants
            .AsNoTracking()
            .Include(ta => ta.User)
            .Where(ta => ta.AvailableSlots > 0 && !ta.IsDeleted)
            .ToListAsync(cancellationToken);

        return Result.Success(await MapWithRatingsAsync(tas, cancellationToken));
    }

    public async Task<Result<IEnumerable<TeamResponse>>> GetSupervisedTeamsAsync(
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Result.Failure<IEnumerable<TeamResponse>>(TaErrors.UserNotFound);

        var ta = await context.TeachingAssistants
            .FirstOrDefaultAsync(ta => ta.UserId.ToString() == CurrentUserId, cancellationToken);

        if (ta is null)
            return Result.Failure<IEnumerable<TeamResponse>>(TaErrors.TaNotFound);

        var teams = await context.Teams
            .AsNoTracking()
            .Include(t => t.Members)
            .Include(t => t.Leader)
            .Where(t => t.TaId == ta.Id)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<TeamResponse>>(teams));
    }

    public async Task<Result> CreateAsync(
        CreateTaRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Result.Failure(TaErrors.UserNotFound);

        var user = await userManager.FindByIdAsync(CurrentUserId);

        if (user is null)
            return Result.Failure(TaErrors.UserNotFound);

        var alreadyHasProfile = await context.TeachingAssistants
            .AnyAsync(ta => ta.UserId.ToString() == CurrentUserId, cancellationToken);

        if (alreadyHasProfile)
            return Result.Failure(TaErrors.UserAlreadyTa);

        var ta = request.Adapt<TeachingAssistant>();

       ta.UserId = CurrentUserId;
        ta.AvailableSlots = request.MaxSlots;

        await context.TeachingAssistants.AddAsync(ta, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(
        Guid id, UpdateTaRequest request, CancellationToken cancellationToken = default)
    {
        var ta = await context.TeachingAssistants
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (ta is null)
            return Result.Failure(TaErrors.TaNotFound);

        request.Adapt(ta);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var ta = await context.TeachingAssistants
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ta is null || ta.IsDeleted)
            return Result.Failure(TaErrors.TaNotFound);

        // Soft-delete the TA record
        ta.IsDeleted = true;

        // Also disable the ApplicationUser so they cannot login/register again
        ta.User.IsDisabled = true;
        await userManager.UpdateAsync(ta.User);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<bool> HasProfileAsync(string userId)
    {
        return await context.TeachingAssistants
            .AnyAsync(x => x.UserId.ToString() == userId);
    }

    private async Task<IEnumerable<TAResponse>> MapWithRatingsAsync(
        IEnumerable<TeachingAssistant> tas, CancellationToken cancellationToken)
    {
        var response = new List<TAResponse>();

        foreach (var ta in tas)
        {
            var avgResult = await ratingService.GetAverageAsync(ta.Id, cancellationToken);

            var taResponse = mapper.Map<TAResponse>(ta) with
            {
                AverageRating = avgResult.IsSuccess ? avgResult.Value : 0
            };

            response.Add(taResponse);
        }

        return response;
    }
}