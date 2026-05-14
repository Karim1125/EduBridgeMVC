namespace EduBridgeMVC.Contracts.Idea;

public record UpdateIdeaTagRequest(
    string? Name,
    Guid? CategoryId
);