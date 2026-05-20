using EduBridgeMVC.Contracts.TA;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBridgeMVC.Controllers;

[Authorize]
[Route("[controller]")]
public class TARequestController(ITaRequestService taRequestService) : Controller
{
    private readonly ITaRequestService _taRequestService = taRequestService;

    [Authorize(Roles = "TA")]
    [HttpGet("incoming/{taId:guid}")]
    public async Task<IActionResult> Incoming(Guid taId, CancellationToken cancellationToken)
    {
        var result = await _taRequestService.GetIncomingRequestsAsync(taId, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(Enumerable.Empty<TaRequestResponse>());
        }

        ViewData["TaId"] = taId;
        ViewData["ListTitle"] = "Incoming Requests";
        return View(result.Value);
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send(Guid teamId, SendTaRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _taRequestService.SendAsync(teamId, request.TAId, request.Message, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "TA request sent successfully"
            : result.Error.Description;

        return RedirectToAction("Details", "Team", new { id = teamId });
    }

    [HttpPost("cancel/{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, Guid teamId, CancellationToken cancellationToken)
    {
        var result = await _taRequestService.CancelAsync(id, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "TA request cancelled"
            : result.Error.Description;

        return RedirectToAction("Details", "Team", new { id = teamId });
    }

    [Authorize(Roles = "TA")]
    [HttpPost("approve/{id:guid}")]
    public async Task<IActionResult> Approve(Guid id, Guid taId, string? responseMessage, CancellationToken cancellationToken)
    {
        var result = await _taRequestService.ApproveAsync(id, responseMessage, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "TA request approved"
            : result.Error.Description;

        return RedirectToAction(nameof(Incoming), new { taId });
    }

    [Authorize(Roles = "TA")]
    [HttpPost("reject/{id:guid}")]
    public async Task<IActionResult> Reject(Guid id, Guid taId, string? responseMessage, CancellationToken cancellationToken)
    {
        var result = await _taRequestService.RejectAsync(id, responseMessage, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "TA request rejected"
            : result.Error.Description;

        return RedirectToAction(nameof(Incoming), new { taId });
    }
}
