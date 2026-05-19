namespace EduBridgeMVC.Contracts.Idea;

public record IdeaResponse(
    Guid Id,
    string Title,
    string Description,
    string? RepositoryUrl,
    Guid CategoryId,
    string CategoryName,
    IEnumerable<IdeaTagDto> Tags,
    Guid TeamId,
    string TeamName,
    DateTime CreatedAt
);

public record IdeaTagDto(
    Guid Id,
    string Name
);