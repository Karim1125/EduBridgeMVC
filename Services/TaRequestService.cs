using System.Security.Claims;
using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Contracts.TA;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class TaRequestService(
    ApplicationDbContext context,
    INotificationService notificationService,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper) : ITaRequestService
{
    private string CurrentUserId => httpContextAccessor.HttpContext!.User
        .FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<Result<IEnumerable<TaRequestResponse>>> GetIncomingRequestsAsync(
        Guid taId, CancellationToken cancellationToken = default)
    {
        var ta = await context.TeachingAssistants
            .FirstOrDefaultAsync(ta => ta.Id == taId, cancellationToken);

        if (ta is null)
            return Result.Failure<IEnumerable<TaRequestResponse>>(TaErrors.TaNotFound);

        if (ta.UserId != CurrentUserId)
            return Result.Failure<IEnumerable<TaRequestResponse>>(TaErrors.NotAuthorized);

        var requests = await context.TaRequests
            .AsNoTracking()
            .Include(tr => tr.Team)
            .Include(tr => tr.TA).ThenInclude(t => t.User)
            .Where(tr => tr.TAId == taId)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<TaRequestResponse>>(requests));
    }

    public async Task<Result<IEnumerable<TaRequestResponse>>> GetTeamRequestsAsync(
        Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await context.Teams
            .FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);

        if (team is null)
            return Result.Failure<IEnumerable<TaRequestResponse>>(TeamErrors.TeamNotFound);

        if (team.LeaderId != CurrentUserId)
            return Result.Failure<IEnumerable<TaRequestResponse>>(TeamErrors.NotTeamLeader);

        var requests = await context.TaRequests
            .AsNoTracking()
            .Include(tr => tr.Team)
            .Include(tr => tr.TA).ThenInclude(t => t.User)
            .Where(tr => tr.TeamId == teamId)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<TaRequestResponse>>(requests));
    }

    public async Task<Result> SendAsync(
        Guid teamId, Guid taId, string? message, CancellationToken cancellationToken = default)
    {
        var team = await context.Teams
            .FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);

        if (team is null)
            return Result.Failure(TeamErrors.TeamNotFound);

        if (team.LeaderId != CurrentUserId)
            return Result.Failure(TeamErrors.NotTeamLeader);

        var ta = await context.TeachingAssistants
            .FirstOrDefaultAsync(ta => ta.Id == taId, cancellationToken);

        if (ta is null)
            return Result.Failure(TaErrors.TaNotFound);

        if (!ta.IsAvailable)
            return Result.Failure(TaErrors.TaNotAvailable);

        var existingRequest = await context.TaRequests
            .AnyAsync(tr => tr.TeamId == teamId
                            && tr.TAId == taId
                            && tr.Status == RequestStatus.Pending, cancellationToken);

        if (existingRequest)
            return Result.Failure(TaRequestErrors.RequestAlreadyExists);

        var request = new TaRequest
        {
            TeamId = teamId,
            TAId = taId,
            Message = message,
            Status = RequestStatus.Pending
        };

        await context.TaRequests.AddAsync(request, cancellationToken);

        team.Status = TeamStatus.TaPending;

        await context.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(
            ta.UserId,
            NotificationType.TaRequestReceived,
            $"Team {team.Name} has requested your supervision",
            request.Id,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> CancelAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var request = await context.TaRequests
            .Include(tr => tr.Team)
            .FirstOrDefaultAsync(tr => tr.Id == id, cancellationToken);

        if (request is null)
            return Result.Failure(TaRequestErrors.RequestNotFound);

        if (request.Team.LeaderId != CurrentUserId)
            return Result.Failure(TeamErrors.NotTeamLeader);

        if (request.Status != RequestStatus.Pending)
            return Result.Failure(TaRequestErrors.RequestNotPending);

        request.Status = RequestStatus.Cancelled;
        request.Team.Status = TeamStatus.Full;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ApproveAsync(
        Guid id, string? responseMessage, CancellationToken cancellationToken = default)
    {
        var request = await context.TaRequests
            .Include(tr => tr.Team)
            .Include(tr => tr.TA)
            .FirstOrDefaultAsync(tr => tr.Id == id, cancellationToken);

        if (request is null)
            return Result.Failure(TaRequestErrors.RequestNotFound);

        if (request.TA.UserId != CurrentUserId)
            return Result.Failure(TaErrors.NotAuthorized);

        if (request.Status != RequestStatus.Pending)
            return Result.Failure(TaRequestErrors.RequestNotPending);

        request.Status = RequestStatus.Approved;
        request.ResponseMessage = responseMessage;
        request.RespondedByTAId = request.TAId;
        request.RespondedAt = DateTime.UtcNow;

        request.Team.TaId = request.TAId;
        request.Team.Status = TeamStatus.TaApproved;
        request.TA.AvailableSlots--;

        await context.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(
            request.Team.LeaderId,
            NotificationType.TaRequestAccepted,
            "Your request for Ta supervision has been accepted",
            request.Id,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RejectAsync(
        Guid id, string? responseMessage, CancellationToken cancellationToken = default)
    {
        var request = await context.TaRequests
            .Include(tr => tr.Team)
            .Include(tr => tr.TA)
            .FirstOrDefaultAsync(tr => tr.Id == id, cancellationToken);

        if (request is null)
            return Result.Failure(TaRequestErrors.RequestNotFound);

        if (request.TA.UserId != CurrentUserId)
            return Result.Failure(TaErrors.NotAuthorized);

        if (request.Status != RequestStatus.Pending)
            return Result.Failure(TaRequestErrors.RequestNotPending);

        request.Status = RequestStatus.Rejected;
        request.ResponseMessage = responseMessage;
        request.RespondedByTAId = request.TAId;
        request.RespondedAt = DateTime.UtcNow;
        request.Team.Status = TeamStatus.Full;

        await context.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(
            request.Team.LeaderId,
            NotificationType.TaRequestRejected,
            "Your request for Ta supervision has been rejected",
            request.Id,
            cancellationToken);

        return Result.Success();
    }
}