using EduBridgeMVC.Abstractions;

namespace EduBridgeMVC.Errors;

public static class DoctorRequestErrors
{
    public static readonly Error RequestNotFound = new(
        "DoctorRequest.NotFound", "Doctor request not found", StatusCodes.Status404NotFound);

    public static readonly Error RequestAlreadyExists = new(
        "DoctorRequest.AlreadyExists", "A pending request already exists for this team and doctor",
        StatusCodes.Status409Conflict);

    public static readonly Error RequestNotPending = new(
        "DoctorRequest.NotPending", "This request has already been responded to", StatusCodes.Status409Conflict);

    public static readonly Error Unauthorized = new(
        "DoctorRequest.Unauthorized", "You are not authorized to respond to this request",
        StatusCodes.Status403Forbidden);
}