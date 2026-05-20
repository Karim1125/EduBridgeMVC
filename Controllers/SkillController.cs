using EduBridgeMVC.Contracts.Skills;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBridgeMVC.Controllers;

[Authorize(Roles = "Admin")]
[Route("[controller]")]
public class SkillController(ISkillService skillService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await skillService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(Enumerable.Empty<SkillResponse>());
        }

        return View(result.Value);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new CreateSkillRequest(string.Empty));

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateSkillRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await skillService.GetOrCreateAsync(request.Name, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Skill created successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await skillService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        var skill = result.Value.FirstOrDefault(s => s.Id == id);
        if (skill is null)
        {
            TempData["Error"] = "Skill not found";
            return RedirectToAction(nameof(Index));
        }

        return View(new UpdateSkillRequest(skill.Name));
    }

    [HttpPost("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, UpdateSkillRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await skillService.UpdateAsync(id, request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Skill updated successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await skillService.DeleteAsync(id, cancellationToken);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            if (!result.IsSuccess)
                return Json(new { success = false, message = result.Error.Description });

            return Json(new { success = true });
        }

        if (!result.IsSuccess)
            TempData["Error"] = result.Error.Description;
        else
            TempData["Success"] = "Skill deleted successfully";

        return RedirectToAction(nameof(Index));
    }
}
