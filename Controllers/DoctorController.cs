using EduBridgeMVC.Contracts.Doctor;
using EduBridgeMVC.Contracts.Team;
using EduBridgeMVC.Extensions;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EduBridgeMVC.Controllers;

[Authorize]
[Route("[controller]")]
public class DoctorController(IDoctorService doctorService, IUserService userService) : Controller
{
    private readonly IDoctorService _doctorService = doctorService;
    private readonly IUserService _userService = userService;

    private bool CanManageProfile(string userId) =>
        User.IsInRole("Admin") || User.GetUserId() == userId;

    // Redirect Doctor users who haven't created a profile yet
    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (User.Identity!.IsAuthenticated && User.IsInRole("Doctor"))
        {
            var userId = User.GetUserId();
            var hasProfile = await _doctorService.HasProfileAsync(userId!);
            var currentAction = context.RouteData.Values["action"]?.ToString()?.ToLower();

            if (!hasProfile && currentAction != "create")
            {
                context.Result = new RedirectToActionResult("Create", "Doctor", null);
                return;
            }
        }

        await next();
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _doctorService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(Enumerable.Empty<DoctorResponse>());
        }

        return View(result.Value);
    }

    [HttpGet("available")]
    public async Task<IActionResult> Available(CancellationToken cancellationToken)
    {
        var result = await _doctorService.GetAvailableDoctorsAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(nameof(Index), Enumerable.Empty<DoctorResponse>());
        }

        ViewData["ListTitle"] = "Available Doctors";
        return View(nameof(Index), result.Value);
    }

    [Authorize(Roles = "Doctor")]
    [HttpGet("supervised-teams")]
    public async Task<IActionResult> SupervisedTeams(CancellationToken cancellationToken)
    {
        var result = await _doctorService.GetSupervisedTeamsAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(Enumerable.Empty<TeamResponse>());
        }

        return View(result.Value);
    }

    [HttpGet("details/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _doctorService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Value);
    }

    [Authorize(Roles = "Doctor")]
    [HttpGet("create")]
    public IActionResult Create() =>
        View(new CreateDoctorRequest(string.Empty, null, null, 0));

    [Authorize(Roles = "Doctor")]
    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateDoctorRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        var userId = User.GetUserId()!;
        var result = await _doctorService.CreateAsync(userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Doctor profile created successfully";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Doctor,Admin")]
    [HttpGet("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _doctorService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        if (!CanManageProfile(result.Value.UserId))
            return Forbid();

        var request = new UpdateDoctorRequest(
            result.Value.Department,
            result.Value.AcademicTitle,
            result.Value.OfficeLocation,
            result.Value.MaxTeams,
            result.Value.AvailableTeams);

        return View(request);
    }

    [Authorize(Roles = "Doctor,Admin")]
    [HttpPost("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        var doctorResult = await _doctorService.GetByIdAsync(id, cancellationToken);
        if (!doctorResult.IsSuccess)
        {
            TempData["Error"] = doctorResult.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        if (!CanManageProfile(doctorResult.Value.UserId))
            return Forbid();

        var result = await _doctorService.UpdateAsync(id, request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Doctor profile updated successfully";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Doctor,Admin")]
    [HttpPost("upload-image/{id:guid}")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile image, CancellationToken cancellationToken)
    {
        if (image == null || image.Length == 0)
        {
            TempData["Error"] = "Please select a valid image file.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var doctorResult = await _doctorService.GetByIdAsync(id, cancellationToken);
        if (!doctorResult.IsSuccess)
        {
            TempData["Error"] = doctorResult.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        if (!CanManageProfile(doctorResult.Value.UserId))
            return Forbid();

        var result = await _userService.UploadProfileImageAsync(doctorResult.Value.UserId, image, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = "Profile image updated successfully";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _doctorService.DeleteAsync(id, cancellationToken);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            if (!result.IsSuccess)
                return Json(new { success = false, message = result.Error.Description });
            return Json(new { success = true });
        }

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Doctor deleted successfully";
        return RedirectToAction(nameof(Index));
    }
}
