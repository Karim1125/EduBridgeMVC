using EduBridgeMVC.Abstractions;

namespace EduBridgeMVC.Errors;

public static class TaRequestErrors
{
    public static readonly Error RequestNotFound = new(
        "TARequest.NotFound", "TA request not found", StatusCodes.Status404NotFound);

    public static readonly Error RequestAlreadyExists = new(
        "TARequest.AlreadyExists", "A pending request already exists for this team and TA",
        StatusCodes.Status409Conflict);

    public static readonly Error RequestNotPending = new(
        "TARequest.NotPending", "This request has already been responded to", StatusCodes.Status409Conflict);
}