using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.Idea;

namespace EduBridgeMVC.Services.Interfaces;

public interface IIdeaTagService
{
    // Queries
    Task<Result<IEnumerable<IdeaTagResponse>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<IdeaTagResponse>>> GetByCategoryAsync(Guid categoryId,
        CancellationToken cancellationToken = default);

    // Operations
    Task<Result<Guid>> GetOrCreateAsync(string name, Guid categoryId, CancellationToken cancellationToken = default);

    // Admin operations
    Task<Result<IdeaTagResponse>> UpdateAsync(Guid id, UpdateIdeaTagRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}