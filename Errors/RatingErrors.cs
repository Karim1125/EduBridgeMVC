using EduBridgeMVC.Abstractions;

namespace EduBridgeMVC.Errors;

public static class RatingErrors
{
    public static readonly Error RatingNotFound = new(
        "Rating.NotFound", "Rating not found", StatusCodes.Status404NotFound);

    public static readonly Error TeamAlreadyRated = new(
        "Rating.AlreadyExists", "This team has already submitted a rating", StatusCodes.Status409Conflict);

    public static readonly Error InvalidScore = new(
        "Rating.InvalidScore", "Score must be between 1 and 100", StatusCodes.Status400BadRequest);

    public static readonly Error TeamHasNoTa = new(
        "Rating.TeamHasNoTa", "This team does not have an assigned TA", StatusCodes.Status400BadRequest);
}