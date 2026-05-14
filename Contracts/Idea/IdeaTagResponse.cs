namespace EduBridgeMVC.Contracts.Idea;

public record IdeaTagResponse(
    Guid Id,
    string Name,
    string CategoryName
);