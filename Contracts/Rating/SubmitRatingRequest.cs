namespace EduBridgeMVC.Contracts.Rating;

public record SubmitRatingRequest(
    int Score,
    string? Feedback
);