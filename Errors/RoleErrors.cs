using EduBridgeMVC.Abstractions;

namespace EduBridgeMVC.Errors;

public static class RoleErrors
{
    public static readonly Error RoleNotFound = new(
        "Role.NotFound", "Role not found", StatusCodes.Status404NotFound);

    public static readonly Error RoleAlreadyExists = new(
        "Role.AlreadyExists", "A role with this name already exists", StatusCodes.Status409Conflict);

    public static readonly Error UserNotFound = new(
        "Role.UserNotFound", "User not found", StatusCodes.Status404NotFound);

    public static readonly Error UserAlreadyInRole = new(
        "Role.UserAlreadyInRole", "User is already assigned this role", StatusCodes.Status409Conflict);

    public static readonly Error UserNotInRole = new(
        "Role.UserNotInRole", "User does not have this role", StatusCodes.Status409Conflict);
}