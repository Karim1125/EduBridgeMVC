using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.TA;

namespace EduBridgeMVC.Services.Interfaces;

public interface ITaRequestService
{
    // Queries
    Task<Result<IEnumerable<TaRequestResponse>>> GetIncomingRequestsAsync(Guid taId,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<TaRequestResponse>>> GetTeamRequestsAsync(Guid teamId,
        CancellationToken cancellationToken = default);

    Task<Result> SendAsync(Guid teamId, Guid taId, string? message,
        CancellationToken cancellationToken = default);

    Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> ApproveAsync(Guid id, string? responseMessage,
        CancellationToken cancellationToken = default);

    Task<Result> RejectAsync(Guid id, string? responseMessage,
        CancellationToken cancellationToken = default);
}