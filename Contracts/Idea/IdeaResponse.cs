namespace EduBridgeMVC.Contracts.Idea;

public record IdeaResponse(
    Guid Id,
    string Title,
    string Description,
    string? RepositoryUrl,
    string CategoryName,
    IEnumerable<string> Tags,
    string TeamName,
    DateTime CreatedAt
);