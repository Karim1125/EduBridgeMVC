using EduBridgeMVC.Contracts.Idea;
using EduBridgeMVC.Extensions;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBridgeMVC.Controllers;

[Route("[controller]")]
public class IdeaController(
    IIdeaService ideaService,
    IIdeaCategoryService categoryService,
    ITeamService teamService) : Controller
{
    private readonly IIdeaService _ideaService = ideaService;
    private readonly IIdeaCategoryService _categoryService = categoryService;
    private readonly ITeamService _teamService = teamService;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        await SetIdeaListPermissionsAsync(cancellationToken);

        var result = await _ideaService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(Enumerable.Empty<IdeaResponse>());
        }

        return View(result.Value);
    }

    [HttpGet("details/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _ideaService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        ViewBag.CanManageIdea = await CanManageIdeaAsync(result.Value.TeamId, cancellationToken);

        return View(result.Value);
    }

    [Authorize]
    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var teamResult = await _teamService.GetByLeadAsync(userId!, cancellationToken);

        if (!teamResult.IsSuccess)
        {
            TempData["Error"] = "You must be a team lead to create an idea";
            return RedirectToAction(nameof(Index));
        }

        var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
        ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Value : [];

        return View(new CreateIdeaRequest(
            TeamId: Guid.Empty,
            Title: string.Empty,
            Description: string.Empty,
            RepositoryUrl: null,
            CategoryId: Guid.Empty,
            Tags: []
        ));
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateIdeaRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
            ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Value : [];
            return View(request);
        }

        var userId = User.GetUserId();
        var teamResult = await _teamService.GetByLeadAsync(userId!, cancellationToken);

        if (!teamResult.IsSuccess)
        {
            ModelState.AddModelError("", "You must be a team lead to create an idea");
            return View(request);
        }

        var result = await _ideaService.CreateAsync(teamResult.Value.Id, request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
            ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Value : [];
            return View(request);
        }

        TempData["Success"] = "Idea created successfully";
        return RedirectToAction(nameof(Details), new { id = result.Value.Id });
    }

    [Authorize]
    [HttpGet("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _ideaService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        var userId = User.GetUserId();
        var teamResult = await _teamService.GetByLeadAsync(userId!, cancellationToken);

        if (!User.IsInRole("Admin") && (!teamResult.IsSuccess || teamResult.Value.Id != result.Value.TeamId))
        {
            TempData["Error"] = "You can only edit ideas in your team";
            return RedirectToAction(nameof(Index));
        }

        var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
        ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Value : [];

        var request = new UpdateIdeaRequest(
            Title: result.Value.Title,
            Description: result.Value.Description,
            RepositoryUrl: result.Value.RepositoryUrl,
            CategoryId: result.Value.CategoryId,
            Tags: result.Value.Tags.Select(t => t.Name).ToList()
        );

        return View(request);
    }

    [Authorize]
    [HttpPost("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, UpdateIdeaRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
            ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Value : [];
            return View(request);
        }

        var ideaResult = await _ideaService.GetByIdAsync(id, cancellationToken);
        if (!ideaResult.IsSuccess)
        {
            TempData["Error"] = ideaResult.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        var userId = User.GetUserId();
        var teamResult = await _teamService.GetByLeadAsync(userId!, cancellationToken);

        if (!User.IsInRole("Admin") && (!teamResult.IsSuccess || teamResult.Value.Id != ideaResult.Value.TeamId))
        {
            ModelState.AddModelError("", "You can only edit ideas in your team");
            return View(request);
        }

        var result = await _ideaService.UpdateAsync(id, request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
            ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Value : [];
            return View(request);
        }

        TempData["Success"] = "Idea updated successfully";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize]
    [HttpPost("delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ideaResult = await _ideaService.GetByIdAsync(id, cancellationToken);
        if (!ideaResult.IsSuccess)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = ideaResult.Error.Description });

            TempData["Error"] = ideaResult.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        if (!User.IsInRole("Admin"))
        {
            var userId = User.GetUserId();
            var teamResult = await _teamService.GetByLeadAsync(userId!, cancellationToken);

            if (!teamResult.IsSuccess || teamResult.Value.Id != ideaResult.Value.TeamId)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "You can only delete ideas in your team" });

                TempData["Error"] = "You can only delete ideas in your team";
                return RedirectToAction(nameof(Index));
            }
        }

        var result = await _ideaService.DeleteAsync(id, cancellationToken);

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

        TempData["Success"] = "Idea deleted successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> ByCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        await SetIdeaListPermissionsAsync(cancellationToken);

        var result = await _ideaService.GetByCategoryAsync(categoryId, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(nameof(Index), Enumerable.Empty<IdeaResponse>());
        }

        return View(nameof(Index), result.Value);
    }

    [HttpGet("tag/{tagId:guid}")]
    public async Task<IActionResult> ByTag(Guid tagId, CancellationToken cancellationToken)
    {
        await SetIdeaListPermissionsAsync(cancellationToken);

        var result = await _ideaService.GetByTagAsync(tagId, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(nameof(Index), Enumerable.Empty<IdeaResponse>());
        }

        return View(nameof(Index), result.Value);
    }

    private async Task SetIdeaListPermissionsAsync(CancellationToken cancellationToken)
    {
        ViewBag.CanCreateIdea = false;
        ViewBag.CurrentUserTeamId = null;

        if (User.Identity?.IsAuthenticated != true || User.IsInRole("Admin"))
            return;

        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return;

        var teamResult = await _teamService.GetByLeadAsync(userId, cancellationToken);
        if (!teamResult.IsSuccess)
            return;

        ViewBag.CanCreateIdea = true;
        ViewBag.CurrentUserTeamId = teamResult.Value.Id;
    }

    private async Task<bool> CanManageIdeaAsync(Guid teamId, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Admin"))
            return true;

        if (User.Identity?.IsAuthenticated != true)
            return false;

        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return false;

        var teamResult = await _teamService.GetByLeadAsync(userId, cancellationToken);
        return teamResult.IsSuccess && teamResult.Value.Id == teamId;
    }
}
