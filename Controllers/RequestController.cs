using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBridgeMVC.Controllers;

[Authorize]
[Route("[controller]")]
public class RequestController(
    ITaService taService,
    IDoctorService doctorService,
    ITaRequestService taRequestService,
    IDoctorRequestService doctorRequestService) : Controller
{
    private readonly ITaService _taService = taService;
    private readonly IDoctorService _doctorService = doctorService;
    private readonly ITaRequestService _taRequestService = taRequestService;
    private readonly IDoctorRequestService _doctorRequestService = doctorRequestService;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (User.IsInRole("Student"))
            return View();

        if (User.IsInRole("TA"))
        {
            var taResult = await _taService.GetCurrentAsync(cancellationToken);
            if (!taResult.IsSuccess)
            {
                TempData["Error"] = taResult.Error.Description;
                return View("~/Views/TARequest/Incoming.cshtml", Enumerable.Empty<EduBridgeMVC.Contracts.TA.TaRequestResponse>());
            }

            var requestsResult = await _taRequestService.GetIncomingRequestsAsync(taResult.Value.Id, cancellationToken);
            if (!requestsResult.IsSuccess)
            {
                TempData["Error"] = requestsResult.Error.Description;
                return View("~/Views/TARequest/Incoming.cshtml", Enumerable.Empty<EduBridgeMVC.Contracts.TA.TaRequestResponse>());
            }

            ViewData["TaId"] = taResult.Value.Id;
            ViewData["ListTitle"] = "Incoming Requests";
            return View("~/Views/TARequest/Incoming.cshtml", requestsResult.Value);
        }

        if (User.IsInRole("Doctor"))
        {
            var doctorResult = await _doctorService.GetCurrentAsync(cancellationToken);
            if (!doctorResult.IsSuccess)
            {
                TempData["Error"] = doctorResult.Error.Description;
                return View("~/Views/DoctorRequest/Incoming.cshtml", Enumerable.Empty<EduBridgeMVC.Contracts.Doctor.DoctorRequestResponse>());
            }

            var requestsResult = await _doctorRequestService.GetDoctorRequestsAsync(doctorResult.Value.Id, cancellationToken);
            if (!requestsResult.IsSuccess)
            {
                TempData["Error"] = requestsResult.Error.Description;
                return View("~/Views/DoctorRequest/Incoming.cshtml", Enumerable.Empty<EduBridgeMVC.Contracts.Doctor.DoctorRequestResponse>());
            }

            ViewData["DoctorId"] = doctorResult.Value.Id;
            ViewData["ListTitle"] = "Incoming Requests";
            return View("~/Views/DoctorRequest/Incoming.cshtml", requestsResult.Value);
        }

        return View();
    }

    [HttpPost("approve/{id:guid}")]
    public async Task<IActionResult> Approve(Guid id, Guid? taId, Guid? doctorId, string? responseMessage, CancellationToken cancellationToken)
    {
        if (taId.HasValue)
        {
            var result = await _taRequestService.ApproveAsync(id, responseMessage, cancellationToken);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
                ? "TA request approved"
                : result.Error.Description;

            return RedirectToAction(nameof(Index));
        }

        if (doctorId.HasValue)
        {
            var result = await _doctorRequestService.ApproveAsync(id, responseMessage, cancellationToken);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
                ? "Doctor request approved"
                : result.Error.Description;

            return RedirectToAction(nameof(Index));
        }

        return BadRequest();
    }

    [HttpPost("reject/{id:guid}")]
    public async Task<IActionResult> Reject(Guid id, Guid? taId, Guid? doctorId, string? responseMessage, CancellationToken cancellationToken)
    {
        if (taId.HasValue)
        {
            var result = await _taRequestService.RejectAsync(id, responseMessage, cancellationToken);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
                ? "TA request rejected"
                : result.Error.Description;

            return RedirectToAction(nameof(Index));
        }

        if (doctorId.HasValue)
        {
            var result = await _doctorRequestService.RejectAsync(id, responseMessage, cancellationToken);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
                ? "Doctor request rejected"
                : result.Error.Description;

            return RedirectToAction(nameof(Index));
        }

        return BadRequest();
    }
}
