namespace EduBridgeMVC.Contracts.Idea;

public record CreateIdeaTagRequest(
    string Name,
    Guid CategoryId
);