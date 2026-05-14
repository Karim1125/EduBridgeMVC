using EduBridgeMVC.Abstractions.Consts;

namespace EduBridgeMVC.Contracts.Notification;

public record NotificationResponse(
    Guid Id,
    string Message,
    NotificationType Type,
    bool IsRead,
    Guid? RelatedEntityId,
    DateTime CreatedAt
);