using EduBridgeMVC.Contracts.User;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBridgeMVC.Controllers;

[Authorize]
[Route("[controller]")]
public class UserController(IUserService userService) : Controller
{
    private readonly IUserService _userService = userService;

    [HttpPost("upload-image/{id}")]
    public async Task<IActionResult> UploadImage(string id, IFormFile image, CancellationToken cancellationToken)
    {
        if (image == null || image.Length == 0)
        {
            TempData["Error"] = "Please select a valid image file.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var result = await _userService.UploadProfileImageAsync(id, image, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = "Profile image updated successfully";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) { var result = await _userService.GetAllUsersAsync(cancellationToken); if (!result.IsSuccess) { TempData["Error"] = result.Error.Description; return View(Enumerable.Empty<UserResponse>()); } return View(result.Value); }

    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetUserByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("create")]
    public IActionResult Create() =>
        View(new CreateUserRequest(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, null, null, null, string.Empty));

    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _userService.AddAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "User created successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var result = await _userService.GetUserByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        var request = new UpdateUserRequest(
            result.Value.FirstName,
            result.Value.LastName,
            null, null, null, null, null);

        return View(request);
    }

    [HttpPost("edit/{id}")]
    public async Task<IActionResult> Edit(string id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _userService.UpdateProfileAsync(id, request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "User updated successfully";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("delete/{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var result = await _userService.DeleteAsync(id, cancellationToken);

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

        TempData["Success"] = "User disabled successfully";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("toggle-disable/{id}")]
    public async Task<IActionResult> ToggleDisable(string id, CancellationToken cancellationToken)
    {
        var result = await _userService.ToggleDisableAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return Json(new { success = false, message = result.Error.Description });

        return Json(new { success = true, isDisabled = result.Value });
    }
}