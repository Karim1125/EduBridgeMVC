using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.Skills;

namespace EduBridgeMVC.Services.Interfaces;

public interface IJoinRequestService
{
    // Queries
    Task<Result<IEnumerable<JoinRequestResponse>>> GetIncomingRequestsAsync(Guid teamId,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<JoinRequestResponse>>> GetUserRequestsAsync(string studentId,
        CancellationToken cancellationToken = default);

    // Student operations
    Task<Result> SendAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken = default);

    // Leader operations
    Task<Result> ApproveAsync(Guid id, string? responseMessage, CancellationToken cancellationToken = default);
    Task<Result> RejectAsync(Guid id, string? responseMessage, CancellationToken cancellationToken = default);
}