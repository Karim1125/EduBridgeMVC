using EduBridgeMVC.Contracts.User;
using EduBridgeMVC.Extensions;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBridgeMVC.Controllers;

[Authorize]
[Route("[controller]")]
public class ProfileController(IUserService userService, ISkillService skillService) : Controller
{
    // GET /Profile  — the "Profile" nav tab
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await userService.GetCurrentUserAsync(userId, cancellationToken);
        var skillsResult = await skillService.GetAllAsync(cancellationToken);
        ViewBag.AllSkills = skillsResult.IsSuccess ? skillsResult.Value : [];

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction("Index", "Home");
        }

        return View(result.Value);
    }

    // GET /Profile/Edit
    [HttpGet("edit")]
    public async Task<IActionResult> Edit(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await userService.GetCurrentUserAsync(userId, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        var p = result.Value;
        return View(new UpdateUserRequest(p.FirstName, p.LastName, p.Bio, p.Major, p.University, p.GitHubUrl, p.LinkedInUrl));
    }

    // POST /Profile/Edit
    [HttpPost("edit")]
    public async Task<IActionResult> Edit(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(request);

        var userId = User.GetUserId()!;
        var result = await userService.UpdateProfileAsync(userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Profile updated successfully";
        return RedirectToAction(nameof(Index));
    }

    // POST /Profile/upload-image  — AJAX image upload
    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(IFormFile image, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;

        if (image == null || image.Length == 0)
            return Json(new { success = false, message = "Please select a valid image." });

        var result = await userService.UploadProfileImageAsync(userId, image, cancellationToken);

        if (!result.IsSuccess)
            return Json(new { success = false, message = result.Error.Description });

        // Return new image URL so JS can update the <img> live
        var profile = await userService.GetCurrentUserAsync(userId, cancellationToken);
        var url = profile.IsSuccess ? profile.Value.ProfileImageUrl ?? "/img/default.png" : "/img/default.png";
        return Json(new { success = true, url });
    }

    // GET /Profile/change-password
    [HttpGet("change-password")]
    public IActionResult ChangePassword() => View(new ChangePasswordRequest(string.Empty, string.Empty, string.Empty));

    // POST /Profile/change-password
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(request);

        var userId = User.GetUserId()!;
        var result = await userService.ChangePasswordAsync(userId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Password changed successfully";
        return RedirectToAction(nameof(Index));
    }

    // POST /Profile/add-skills  — AJAX
    [HttpPost("add-skills")]
    public async Task<IActionResult> AddSkills([FromBody] List<string> skills, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await userService.AddSkillsAsync(userId, skills, cancellationToken);
        return Json(result.IsSuccess ? new { success = true } : new { success = false, message = result.Error.Description });
    }

    // POST /Profile/remove-skill/{skillId}
    [HttpPost("remove-skill/{skillId:guid}")]
    public async Task<IActionResult> RemoveSkill(Guid skillId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await userService.RemoveSkillAsync(userId, skillId, cancellationToken);
        return Json(result.IsSuccess ? new { success = true } : new { success = false, message = result.Error.Description });
    }
}
