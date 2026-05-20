using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.TA;
using EduBridgeMVC.Contracts.Team;

namespace EduBridgeMVC.Services.Interfaces;

public interface ITaService
{
    // Queries
    Task<Result<IEnumerable<TAResponse>>> GetAllTAsAsync(CancellationToken cancellationToken = default);
    Task<Result<TAResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TAResponse>> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TAResponse>>> GetAvailableTAsAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TeamResponse>>> GetSupervisedTeamsAsync(CancellationToken cancellationToken = default);

    // Commands
    Task<Result> CreateAsync(CreateTaRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(Guid id, UpdateTaRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasProfileAsync(string userId);
}
