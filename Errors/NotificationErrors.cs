using EduBridgeMVC.Abstractions;

namespace EduBridgeMVC.Errors;

public static class NotificationErrors
{
    public static readonly Error NotificationNotFound = new(
        "Notification.NotFound", "Notification not found", StatusCodes.Status404NotFound);
}