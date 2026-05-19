using EduBridgeMVC.Contracts.Notification;
using EduBridgeMVC.Extensions;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBridgeMVC.Controllers;

[Authorize]
[Route("[controller]")]
public class NotificationController(INotificationService notificationService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await notificationService.GetUserNotificationsAsync(userId, cancellationToken);
        var unreadResult = await notificationService.GetUnreadCountAsync(userId, cancellationToken);

        ViewBag.UnreadCount = unreadResult.IsSuccess ? unreadResult.Value : 0;

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(Enumerable.Empty<NotificationResponse>());
        }

        return View(result.Value);
    }

    [HttpPost("mark-read/{id:guid}")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await notificationService.MarkAsReadAsync(id, userId, cancellationToken);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            if (!result.IsSuccess)
                return Json(new { success = false, message = result.Error.Description });

            return Json(new { success = true });
        }

        if (!result.IsSuccess)
            TempData["Error"] = result.Error.Description;
        else
            TempData["Success"] = "Notification marked as read";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await notificationService.MarkAllAsReadAsync(userId, cancellationToken);

        if (!result.IsSuccess)
            TempData["Error"] = result.Error.Description;
        else
            TempData["Success"] = "All notifications marked as read";

        return RedirectToAction(nameof(Index));
    }
}
