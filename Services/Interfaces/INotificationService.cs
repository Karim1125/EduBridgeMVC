using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.Notification;
using NotificationType = EduBridgeMVC.Abstractions.Consts.NotificationType;

namespace EduBridgeMVC.Services.Interfaces;

public interface INotificationService
{
    // Queries
    Task<Result<IEnumerable<NotificationResponse>>> GetUserNotificationsAsync(string userId,
        CancellationToken cancellationToken = default);

    Task<Result<int>> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);

    // Operations
    Task<Result> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);

    // Internal - called by other services
    Task<Result> SendAsync(string userId, NotificationType type, string message, Guid? relatedEntityId,
        CancellationToken cancellationToken = default);
}