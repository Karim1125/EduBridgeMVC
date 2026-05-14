namespace EduBridgeMVC.Contracts.Rating;

public record RatingResponse(
    Guid Id,
    Guid TeamId,
    string TeamName,
    string TaName,
    int Score,
    string? Feedback,
    DateTime CreatedAt
);