using EduBridgeMVC.Abstractions;

namespace EduBridgeMVC.Errors;

public static class TaErrors
{
    public static readonly Error TaNotFound = new(
        "TA.NotFound", "Teaching assistant not found", StatusCodes.Status404NotFound);

    public static readonly Error TaNotAvailable = new(
        "TA.NotAvailable", "This teaching assistant has no available slots", StatusCodes.Status409Conflict);

    public static readonly Error NotAuthorized = new(
        "TA.NotAuthorized", "You are not authorized to perform this action", StatusCodes.Status403Forbidden);

    public static readonly Error UserNotFound = new(
        "TA.UserNotFound", "User not found", StatusCodes.Status404NotFound);

    public static readonly Error UserNotTaRole = new(
        "TA.NotTaRole", "User does not have the TA role", StatusCodes.Status403Forbidden);

    public static readonly Error UserAlreadyTa = new(
        "TA.AlreadyExists", "This user is already registered as a TA", StatusCodes.Status409Conflict);
}