namespace EduBridgeMVC.Contracts.Idea;

public record CreateIdeaRequest(
    Guid TeamId,
    string Title,
    string Description,
    string? RepositoryUrl,
    Guid CategoryId,
    List<string> Tags
);