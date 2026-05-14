using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.Doctor;

namespace EduBridgeMVC.Services.Interfaces;

public interface IDoctorRequestService
{
    // Create request
    Task<Result> CreateRequestAsync(SendDoctorRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result> CancelRequestAsync(Guid requestId, CancellationToken cancellationToken = default);

    Task<Result> ApproveAsync(Guid requestId, string? responseMessage, CancellationToken cancellationToken = default);
    Task<Result> RejectAsync(Guid requestId, string? responseMessage, CancellationToken cancellationToken = default);

    // Queries
    Task<Result<IEnumerable<DoctorRequestResponse>>> GetTeamRequestsAsync(Guid teamId,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<DoctorRequestResponse>>> GetDoctorRequestsAsync(Guid doctorId,
        CancellationToken cancellationToken = default);
}