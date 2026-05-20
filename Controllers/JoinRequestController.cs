using EduBridgeMVC.Contracts.Skills;
using EduBridgeMVC.Extensions;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBridgeMVC.Controllers;

[Authorize]
[Route("[controller]")]
public class JoinRequestController(IJoinRequestService joinRequestService) : Controller
{
    private readonly IJoinRequestService _joinRequestService = joinRequestService;

    [HttpGet("incoming/{teamId:guid}")]
    public async Task<IActionResult> Incoming(Guid teamId, CancellationToken cancellationToken)
    {
        var result = await _joinRequestService.GetIncomingRequestsAsync(teamId, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(Enumerable.Empty<JoinRequestResponse>());
        }

        ViewData["TeamId"] = teamId;
        ViewData["ListTitle"] = "Join Requests";
        return View(result.Value);
    }

    [HttpGet("my")]
    public async Task<IActionResult> MyRequests(CancellationToken cancellationToken)
    {
        var result = await _joinRequestService.GetUserRequestsAsync(User.GetUserId()!, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(Enumerable.Empty<JoinRequestResponse>());
        }

        ViewData["ListTitle"] = "My Join Requests";
        return View(nameof(Incoming), result.Value);
    }

    [Authorize(Roles = "Student")]
    [HttpPost("send/{teamId:guid}")]
    public async Task<IActionResult> Send(Guid teamId, CancellationToken cancellationToken)
    {
        var result = await _joinRequestService.SendAsync(teamId, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Join request sent successfully"
            : result.Error.Description;

        return RedirectToAction("Details", "Team", new { id = teamId });
    }

    [HttpPost("cancel/{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, Guid teamId, CancellationToken cancellationToken)
    {
        var result = await _joinRequestService.CancelAsync(id, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Join request cancelled"
            : result.Error.Description;

        return teamId == Guid.Empty
            ? RedirectToAction(nameof(MyRequests))
            : RedirectToAction("Details", "Team", new { id = teamId });
    }

    [HttpPost("approve/{id:guid}")]
    public async Task<IActionResult> Approve(Guid id, Guid teamId, string? responseMessage, CancellationToken cancellationToken)
    {
        var result = await _joinRequestService.ApproveAsync(id, responseMessage, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Join request approved"
            : result.Error.Description;

        return RedirectToAction(nameof(Incoming), new { teamId });
    }

    [HttpPost("reject/{id:guid}")]
    public async Task<IActionResult> Reject(Guid id, Guid teamId, string? responseMessage, CancellationToken cancellationToken)
    {
        var result = await _joinRequestService.RejectAsync(id, responseMessage, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Join request rejected"
            : result.Error.Description;

        return RedirectToAction(nameof(Incoming), new { teamId });
    }
}
