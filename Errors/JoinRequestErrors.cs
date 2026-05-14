using EduBridgeMVC.Abstractions;

namespace EduBridgeMVC.Errors;

public static class JoinRequestErrors
{
    public static readonly Error RequestNotFound = new(
        "JoinRequest.NotFound", "Join request not found", StatusCodes.Status404NotFound);

    public static readonly Error RequestAlreadyExists = new(
        "JoinRequest.AlreadyExists", "A pending request already exists for this team", StatusCodes.Status409Conflict);

    public static readonly Error RequestNotPending = new(
        "JoinRequest.NotPending", "This request has already been responded to", StatusCodes.Status409Conflict);

    public static readonly Error AlreadyMember = new(
        "JoinRequest.AlreadyMember", "You are already a member of this team", StatusCodes.Status409Conflict);

    public static readonly Error NotRequestOwner = new(
        "JoinRequest.NotOwner", "You are not the owner of this request", StatusCodes.Status403Forbidden);
}