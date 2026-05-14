using System.Security.Claims;
using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Contracts.Doctor;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class DoctorRequestService(
    ApplicationDbContext context,
    INotificationService notificationService,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper) : IDoctorRequestService
{
    private string CurrentUserId => httpContextAccessor.HttpContext!.User
        .FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<Result> CreateRequestAsync(
        SendDoctorRequestRequest request, CancellationToken cancellationToken = default)
    {
        var doctor = await context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

        if (doctor is null)
            return Result.Failure(DoctorErrors.DoctorNotFound);

        if (!doctor.IsAvailable)
            return Result.Failure(DoctorErrors.DoctorNotAvailable);

        var existingRequest = await context.DoctorRequests
            .AnyAsync(r => r.TeamId == request.TeamId
                           && r.DoctorId == request.DoctorId
                           && r.Status == RequestStatus.Pending, cancellationToken);

        if (existingRequest)
            return Result.Failure(DoctorRequestErrors.RequestAlreadyExists);

        var doctorRequest = new DoctorRequest
        {
            TeamId = request.TeamId,
            DoctorId = request.DoctorId,
            Message = request.Message,
            Status = RequestStatus.Pending
        };

        await context.DoctorRequests.AddAsync(doctorRequest, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(
            doctor.UserId,
            NotificationType.DoctorRequestReceived,
            "You have received a new doctor request from a team.",
            doctorRequest.Id,
            cancellationToken);

        return Result.Success();
    }
    // ... existing methods

    public async Task<Result> CancelRequestAsync(
        Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await context.DoctorRequests
            .Include(r => r.Team)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (request is null)
            return Result.Failure(DoctorRequestErrors.RequestNotFound);

        if (request.Team.LeaderId != CurrentUserId)
            return Result.Failure(TeamErrors.NotTeamLeader);

        if (request.Status != RequestStatus.Pending)
            return Result.Failure(DoctorRequestErrors.RequestNotPending);

        request.Status = RequestStatus.Cancelled;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ApproveAsync(
        Guid requestId, string? responseMessage, CancellationToken cancellationToken = default)
    {
        var doctorRequest = await context.DoctorRequests
            .Include(r => r.Doctor)
            .Include(r => r.Team)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (doctorRequest is null)
            return Result.Failure(DoctorRequestErrors.RequestNotFound);

        if (doctorRequest.Doctor.UserId != CurrentUserId)
            return Result.Failure(DoctorRequestErrors.Unauthorized);

        if (doctorRequest.Status != RequestStatus.Pending)
            return Result.Failure(DoctorRequestErrors.RequestNotPending);

        doctorRequest.Status = RequestStatus.Approved;
        doctorRequest.ResponseMessage = responseMessage;
        doctorRequest.RespondedAt = DateTime.UtcNow;
        doctorRequest.RespondedByDoctorId = doctorRequest.DoctorId;

        doctorRequest.Doctor.AvailableTeams--;
        doctorRequest.Team.DoctorId = doctorRequest.DoctorId;

        await context.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(
            doctorRequest.Team.LeaderId,
            NotificationType.DoctorRequestReceived,
            "Your request for doctor supervision has been accepted",
            doctorRequest.Id,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RejectAsync(
        Guid requestId, string? responseMessage, CancellationToken cancellationToken = default)
    {
        var doctorRequest = await context.DoctorRequests
            .Include(r => r.Doctor)
            .Include(r => r.Team)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (doctorRequest is null)
            return Result.Failure(DoctorRequestErrors.RequestNotFound);

        if (doctorRequest.Doctor.UserId != CurrentUserId)
            return Result.Failure(DoctorRequestErrors.Unauthorized);

        if (doctorRequest.Status != RequestStatus.Pending)
            return Result.Failure(DoctorRequestErrors.RequestNotPending);

        doctorRequest.Status = RequestStatus.Rejected;
        doctorRequest.ResponseMessage = responseMessage;
        doctorRequest.RespondedAt = DateTime.UtcNow;
        doctorRequest.RespondedByDoctorId = doctorRequest.DoctorId;

        await context.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(
            doctorRequest.Team.LeaderId,
            NotificationType.DoctorRequestReceived,
            "Your request for doctor supervision has been rejected",
            doctorRequest.Id,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<DoctorRequestResponse>>> GetTeamRequestsAsync(
        Guid teamId, CancellationToken cancellationToken = default)
    {
        var requests = await context.DoctorRequests
            .AsNoTracking()
            .Include(r => r.Team)
            .Include(r => r.Doctor).ThenInclude(d => d.User)
            .Where(r => r.TeamId == teamId)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<DoctorRequestResponse>>(requests));
    }

    public async Task<Result<IEnumerable<DoctorRequestResponse>>> GetDoctorRequestsAsync(
        Guid doctorId, CancellationToken cancellationToken = default)
    {
        var requests = await context.DoctorRequests
            .AsNoTracking()
            .Include(r => r.Team)
            .Include(r => r.Doctor).ThenInclude(d => d.User)
            .Where(r => r.DoctorId == doctorId)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<DoctorRequestResponse>>(requests));
    }
}