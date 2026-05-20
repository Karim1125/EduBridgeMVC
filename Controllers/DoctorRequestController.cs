using EduBridgeMVC.Contracts.Doctor;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBridgeMVC.Controllers;

[Authorize]
[Route("[controller]")]
public class DoctorRequestController(IDoctorRequestService doctorRequestService) : Controller
{
    private readonly IDoctorRequestService _doctorRequestService = doctorRequestService;

    [Authorize(Roles = "Doctor")]
    [HttpGet("incoming/{doctorId:guid}")]
    public async Task<IActionResult> Incoming(Guid doctorId, CancellationToken cancellationToken)
    {
        var result = await _doctorRequestService.GetDoctorRequestsAsync(doctorId, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(Enumerable.Empty<DoctorRequestResponse>());
        }

        ViewData["DoctorId"] = doctorId;
        ViewData["ListTitle"] = "Incoming Requests";
        return View(result.Value);
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send(SendDoctorRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _doctorRequestService.CreateRequestAsync(request, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Doctor request sent successfully"
            : result.Error.Description;

        return RedirectToAction("Details", "Team", new { id = request.TeamId });
    }

    [HttpPost("cancel/{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, Guid teamId, CancellationToken cancellationToken)
    {
        var result = await _doctorRequestService.CancelRequestAsync(id, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Doctor request cancelled"
            : result.Error.Description;

        return RedirectToAction("Details", "Team", new { id = teamId });
    }

    [Authorize(Roles = "Doctor")]
    [HttpPost("approve/{id:guid}")]
    public async Task<IActionResult> Approve(Guid id, Guid doctorId, string? responseMessage, CancellationToken cancellationToken)
    {
        var result = await _doctorRequestService.ApproveAsync(id, responseMessage, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Doctor request approved"
            : result.Error.Description;

        return RedirectToAction(nameof(Incoming), new { doctorId });
    }

    [Authorize(Roles = "Doctor")]
    [HttpPost("reject/{id:guid}")]
    public async Task<IActionResult> Reject(Guid id, Guid doctorId, string? responseMessage, CancellationToken cancellationToken)
    {
        var result = await _doctorRequestService.RejectAsync(id, responseMessage, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Doctor request rejected"
            : result.Error.Description;

        return RedirectToAction(nameof(Incoming), new { doctorId });
    }
}
