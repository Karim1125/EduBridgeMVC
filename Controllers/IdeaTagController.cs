using EduBridgeMVC.Contracts.Idea;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBridgeMVC.Controllers;

[Authorize(Roles = "Admin")]
[Route("[controller]")]
public class IdeaTagController(
    IIdeaTagService tagService,
    IIdeaCategoryService categoryService) : Controller
{
    private readonly IIdeaTagService _tagService = tagService;
    private readonly IIdeaCategoryService _categoryService = categoryService;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _tagService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(Enumerable.Empty<IdeaTagResponse>());
        }

        return View(result.Value);
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> ByCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        var result = await _tagService.GetByCategoryAsync(categoryId, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(nameof(Index), Enumerable.Empty<IdeaTagResponse>());
        }

        return View(nameof(Index), result.Value);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
        ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Value : [];

        return View(new CreateIdeaTagRequest(
            Name: string.Empty,
            CategoryId: Guid.Empty
        ));
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateIdeaTagRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
            ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Value : [];
            return View(request);
        }

        var result = await _tagService.GetOrCreateAsync(request.Name, request.CategoryId, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
            ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Value : [];
            return View(request);
        }

        TempData["Success"] = "Idea tag created successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var allTags = await _tagService.GetAllAsync(cancellationToken);

        if (!allTags.IsSuccess)
        {
            TempData["Error"] = allTags.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        var tag = allTags.Value.FirstOrDefault(t => t.Id == id);
        if (tag is null)
        {
            TempData["Error"] = "Tag not found";
            return RedirectToAction(nameof(Index));
        }

        var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
        ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Value : [];

        var request = new UpdateIdeaTagRequest(
            Name: tag.Name,
            CategoryId: tag.CategoryId
        );

        return View(request);
    }

    [HttpPost("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, UpdateIdeaTagRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
            ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Value : [];
            return View(request);
        }

        var result = await _tagService.UpdateAsync(id, request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
            ViewBag.Categories = categoriesResult.IsSuccess ? categoriesResult.Value : [];
            return View(request);
        }

        TempData["Success"] = "Idea tag updated successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _tagService.DeleteAsync(id, cancellationToken);

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

        TempData["Success"] = "Idea tag deleted successfully";
        return RedirectToAction(nameof(Index));
    }
}
