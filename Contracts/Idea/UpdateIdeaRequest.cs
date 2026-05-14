namespace EduBridgeMVC.Contracts.Idea;

public record UpdateIdeaRequest(
    string? Title,
    string? Description,
    string? RepositoryUrl,
    Guid? CategoryId,
    List<string>? Tags
);