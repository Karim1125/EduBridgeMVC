using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Contracts.Notification;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class NotificationService(
    ApplicationDbContext context,
    IMapper mapper) : INotificationService
{
    public async Task<Result<IEnumerable<NotificationResponse>>> GetUserNotificationsAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var notifications = await context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<NotificationResponse>>(notifications));
    }

    public async Task<Result<int>> GetUnreadCountAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        var count = await context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);

        return Result.Success(count);
    }

    public async Task<Result> MarkAsReadAsync(
        Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);

        if (notification is null || notification.IsDeleted)
            return Result.Failure(NotificationErrors.NotificationNotFound);

        notification.IsRead = true;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> MarkAllAsReadAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        await context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);

        return Result.Success();
    }

    public async Task<Result> SendAsync(string userId, NotificationType type, string message,
        Guid? relatedEntityId, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            RelatedEntityId = relatedEntityId,
            IsRead = false
        };

        await context.Notifications.AddAsync(notification, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
