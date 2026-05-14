using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.Idea;

namespace EduBridgeMVC.Services.Interfaces;

public interface IIdeaCategoryService
{
    Task<Result<IEnumerable<IdeaCategoryResponse>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<Guid>> GetOrCreateAsync(CreateIdeaCategoryRequest request,
        CancellationToken cancellationToken = default);

    // Admin operations
    Task<Result<IdeaCategoryResponse>> UpdateAsync(Guid id, UpdateIdeaCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}