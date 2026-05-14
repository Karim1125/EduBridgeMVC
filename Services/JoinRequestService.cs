using System.Security.Claims;
using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Contracts.Skills;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class JoinRequestService(
    ApplicationDbContext context,
    INotificationService notificationService,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper) : IJoinRequestService
{
    private string CurrentUserId => httpContextAccessor.HttpContext!.User
        .FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<Result<IEnumerable<JoinRequestResponse>>> GetIncomingRequestsAsync(
        Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await context.Teams
            .FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);

        if (team is null)
            return Result.Failure<IEnumerable<JoinRequestResponse>>(TeamErrors.TeamNotFound);

        if (team.LeaderId != CurrentUserId)
            return Result.Failure<IEnumerable<JoinRequestResponse>>(TeamErrors.NotTeamLeader);

        var requests = await context.JoinRequests
            .AsNoTracking()
            .Include(jr => jr.Team)
            .Include(jr => jr.Student)
            .Where(jr => jr.TeamId == teamId)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<JoinRequestResponse>>(requests));
    }

    public async Task<Result<IEnumerable<JoinRequestResponse>>> GetUserRequestsAsync(
        string studentId, CancellationToken cancellationToken = default)
    {
        var requests = await context.JoinRequests
            .AsNoTracking()
            .Include(jr => jr.Team)
            .Include(jr => jr.Student)
            .Where(jr => jr.StudentId == studentId)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<JoinRequestResponse>>(requests));
    }

    public async Task<Result> SendAsync(
        Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);

        if (team is null)
            return Result.Failure(TeamErrors.TeamNotFound);

        if (team.Members.Any(m => m.UserId == CurrentUserId))
            return Result.Failure(JoinRequestErrors.AlreadyMember);

        if (team.Status == TeamStatus.Full)
            return Result.Failure(TeamErrors.TeamFull);

        var existingRequest = await context.JoinRequests
            .AnyAsync(jr => jr.TeamId == teamId
                            && jr.StudentId == CurrentUserId
                            && jr.Status == RequestStatus.Pending, cancellationToken);

        if (existingRequest)
            return Result.Failure(JoinRequestErrors.RequestAlreadyExists);

        var request = new JoinRequest
        {
            TeamId = teamId,
            StudentId = CurrentUserId,
            Status = RequestStatus.Pending
        };

        await context.JoinRequests.AddAsync(request, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(
            team.LeaderId,
            NotificationType.JoinRequestReceived,
            $"New join request for team {team.Name}",
            request.Id,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> CancelAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var request = await context.JoinRequests
            .FirstOrDefaultAsync(jr => jr.Id == id, cancellationToken);

        if (request is null)
            return Result.Failure(JoinRequestErrors.RequestNotFound);

        if (request.StudentId != CurrentUserId)
            return Result.Failure(JoinRequestErrors.NotRequestOwner);

        if (request.Status != RequestStatus.Pending)
            return Result.Failure(JoinRequestErrors.RequestNotPending);

        request.Status = RequestStatus.Cancelled;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ApproveAsync(
        Guid id, string? responseMessage, CancellationToken cancellationToken = default)
    {
        var request = await context.JoinRequests
            .Include(jr => jr.Team).ThenInclude(t => t.Members)
            .FirstOrDefaultAsync(jr => jr.Id == id, cancellationToken);

        if (request is null)
            return Result.Failure(JoinRequestErrors.RequestNotFound);

        if (request.Team.LeaderId != CurrentUserId)
            return Result.Failure(TeamErrors.NotTeamLeader);

        if (request.Status != RequestStatus.Pending)
            return Result.Failure(JoinRequestErrors.RequestNotPending);

        if (request.Team.Members.Count >= request.Team.MaxMembers)
            return Result.Failure(TeamErrors.TeamFull);

        request.Status = RequestStatus.Approved;
        request.ResponseMessage = responseMessage;
        request.RespondedById = CurrentUserId;
        request.RespondedAt = DateTime.UtcNow;

        await context.TeamMembers.AddAsync(new TeamMember
        {
            TeamId = request.TeamId,
            UserId = request.StudentId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow
        }, cancellationToken);

        request.Team.Status = request.Team.Members.Count + 1 >= request.Team.MaxMembers
            ? TeamStatus.Full
            : TeamStatus.Partial;

        await context.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(
            request.StudentId,
            NotificationType.JoinRequestAccepted,
            $"Your request to join {request.Team.Name} has been accepted",
            request.Id,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RejectAsync(
        Guid id, string? responseMessage, CancellationToken cancellationToken = default)
    {
        var request = await context.JoinRequests
            .Include(jr => jr.Team)
            .FirstOrDefaultAsync(jr => jr.Id == id, cancellationToken);

        if (request is null)
            return Result.Failure(JoinRequestErrors.RequestNotFound);

        if (request.Team.LeaderId != CurrentUserId)
            return Result.Failure(TeamErrors.NotTeamLeader);

        if (request.Status != RequestStatus.Pending)
            return Result.Failure(JoinRequestErrors.RequestNotPending);

        request.Status = RequestStatus.Rejected;
        request.ResponseMessage = responseMessage;
        request.RespondedById = CurrentUserId;
        request.RespondedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(
            request.StudentId,
            NotificationType.JoinRequestRejected,
            $"Your request to join {request.Team.Name} has been rejected",
            request.Id,
            cancellationToken);

        return Result.Success();
    }
}