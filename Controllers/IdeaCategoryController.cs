using EduBridgeMVC.Contracts.Idea;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBridgeMVC.Controllers;

[Authorize(Roles = "Admin")]
[Route("[controller]")]
public class IdeaCategoryController(IIdeaCategoryService categoryService) : Controller
{
    private readonly IIdeaCategoryService _categoryService = categoryService;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return View(Enumerable.Empty<IdeaCategoryResponse>());
        }

        return View(result.Value);
    }

    [HttpGet("create")]
    public IActionResult Create() =>
        View(new CreateIdeaCategoryRequest(string.Empty));

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateIdeaCategoryRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _categoryService.GetOrCreateAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Idea category created successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        var category = result.Value.FirstOrDefault(c => c.Id == id);
        if (category is null)
        {
            TempData["Error"] = "Category not found";
            return RedirectToAction(nameof(Index));
        }

        var request = new UpdateIdeaCategoryRequest(category.Name);
        return View(request);
    }

    [HttpPost("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, UpdateIdeaCategoryRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _categoryService.UpdateAsync(id, request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Idea category updated successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteAsync(id, cancellationToken);

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

        TempData["Success"] = "Idea category deleted successfully";
        return RedirectToAction(nameof(Index));
    }
}
