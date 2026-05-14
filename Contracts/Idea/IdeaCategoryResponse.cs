namespace EduBridgeMVC.Contracts.Idea;

public record IdeaCategoryResponse(
    Guid Id,
    string Name,
    IEnumerable<string> Tags
);