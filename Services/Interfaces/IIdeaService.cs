using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.Idea;

namespace EduBridgeMVC.Services.Interfaces;

public interface IIdeaService
{
    // Queries
    Task<Result<IdeaResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<IdeaResponse>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<IdeaResponse>>> GetByCategoryAsync(Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<IdeaResponse>>> GetByTagAsync(Guid tagId, CancellationToken cancellationToken = default);

    // Team leader operations
    Task<Result<IdeaResponse>> CreateAsync(Guid teamId, CreateIdeaRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IdeaResponse>> UpdateAsync(Guid id, UpdateIdeaRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}