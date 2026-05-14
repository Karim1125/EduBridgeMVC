using EduBridgeMVC.Contracts.TA;
using EduBridgeMVC.Extensions;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EduBridgeMVC.Controllers;

[Authorize(Roles = "TA,Admin")]
[Route("[controller]")]
public class TaController(ITaService taService, IUserService userService) : Controller
{
    private readonly ITaService _taService = taService;
    private readonly IUserService _userService = userService;

    public override async Task OnActionExecutionAsync(
    ActionExecutingContext context,
    ActionExecutionDelegate next)
    {
        if (User.Identity!.IsAuthenticated && User.IsInRole("TA"))
        {
            var userId = User.GetUserId();

            var hasProfile = await _taService.HasProfileAsync(userId!);

            var currentAction = context.RouteData.Values["action"]?.ToString()?.ToLower();

            if (!hasProfile && currentAction != "create")
            {
                context.Result = new RedirectToActionResult("Create", "Ta", null);
                return;
            }
        }

        await next();
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _taService.GetAllTAsAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(Enumerable.Empty<TAResponse>());
        }

        return View(result.Value);
    }

    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Value);
    }

    [HttpGet("create")]
    public IActionResult Create() =>
        View(new CreateTaRequest(string.Empty, null, null, 0));

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateTaRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        // 🔥 التعديل هنا
        var result = await _taService.CreateAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "TA profile created successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        var request = new UpdateTaRequest(
            result.Value.Department,
            result.Value.AcademicTitle,
            result.Value.OfficeLocation,
            result.Value.MaxSlots,
            result.Value.AvailableSlots);

        return View(request);
    }

    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Edit(Guid id, UpdateTaRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _taService.UpdateAsync(id, request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "TA updated successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("upload-image/{id}")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile image, CancellationToken cancellationToken)
    {
        if (image == null || image.Length == 0)
        {
            TempData["Error"] = "Please select a valid image file.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var taResult = await _taService.GetByIdAsync(id, cancellationToken);
        if (!taResult.IsSuccess)
        {
            TempData["Error"] = taResult.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        var result = await _userService.UploadProfileImageAsync(taResult.Value.UserId, image, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = "Profile image updated successfully";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taService.DeleteAsync(id, cancellationToken);

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

        TempData["Success"] = "TA deleted successfully";
        return RedirectToAction(nameof(Index));
    }
}