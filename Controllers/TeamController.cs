using EduBridgeMVC.Contracts.Team;
using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Extensions;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBridgeMVC.Controllers;

[Authorize]
[Route("[controller]")]
public class TeamController(
    ITeamService teamService,
    ITaService taService,
    IDoctorService doctorService,
    IUserService userService) : Controller
{
    private readonly ITeamService _teamService = teamService;
    private readonly ITaService _taService = taService;
    private readonly IDoctorService _doctorService = doctorService;
    private readonly IUserService _userService = userService;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _teamService.GetAllAsync(cancellationToken);

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
        var result = await _teamService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        if (result.Value.LeaderId == User.GetUserId())
            await LoadRequestProfilesAsync(cancellationToken);

        return View(result.Value);
    }

    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> Members(Guid id, CancellationToken cancellationToken)
    {
        var teamResult = await _teamService.GetByIdAsync(id, cancellationToken);

        if (!teamResult.IsSuccess)
        {
            TempData["Error"] = teamResult.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        var membersResult = await _teamService.GetMembersAsync(id, cancellationToken);

        if (!membersResult.IsSuccess)
        {
            TempData["Error"] = membersResult.Error.Description;
            return RedirectToAction(nameof(Details), new { id });
        }

        ViewData["TeamId"] = id;
        ViewData["TeamName"] = teamResult.Value.Name;
        ViewData["LeaderId"] = teamResult.Value.LeaderId;
        ViewData["CanManageMembers"] = teamResult.Value.LeaderId == User.GetUserId();

        return View(membersResult.Value);
    }

    [Authorize(Roles = "Student")]
    [HttpGet("create")]
    public IActionResult Create() => View(new CreateTeamRequest(string.Empty, null));

    [Authorize(Roles = "Student")]
    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateTeamRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _teamService.CreateAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Team created successfully";
        return RedirectToAction(nameof(Details), new { id = result.Value.Id });
    }

    [HttpGet("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _teamService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }

        if (result.Value.LeaderId != User.GetUserId())
            return Forbid();

        return View(new UpdateTeamRequest(result.Value.Name, result.Value.Description));
    }

    [HttpPost("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, UpdateTeamRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _teamService.UpdateAsync(id, request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Team updated successfully";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _teamService.DeleteAsync(id, cancellationToken);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            if (!result.IsSuccess)
                return Json(new { success = false, message = result.Error.Description });

            return Json(new { success = true });
        }

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Success"] = "Team deleted successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/add-member")]
    public async Task<IActionResult> AddMember(Guid id, string userId, CancellationToken cancellationToken)
    {
        var result = await _teamService.AddMemberAsync(id, userId, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Member added successfully"
            : result.Error.Description;

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:guid}/remove-member")]
    public async Task<IActionResult> RemoveMember(Guid id, string userId, bool fromMembers, CancellationToken cancellationToken)
    {
        var result = await _teamService.RemoveMemberAsync(id, userId, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Member removed successfully"
            : result.Error.Description;

        return RedirectToAction(fromMembers ? nameof(Members) : nameof(Details), new { id });
    }

    [HttpPost("{id:guid}/assign-leader")]
    public async Task<IActionResult> AssignLeader(Guid id, string userId, bool fromMembers, CancellationToken cancellationToken)
    {
        var result = await _teamService.AssignLeaderAsync(id, userId, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Leader updated successfully"
            : result.Error.Description;

        return RedirectToAction(fromMembers ? nameof(Members) : nameof(Details), new { id });
    }

    [HttpPost("{id:guid}/leave")]
    public async Task<IActionResult> Leave(Guid id, CancellationToken cancellationToken)
    {
        var result = await _teamService.LeaveAsync(id, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "You left the team"
            : result.Error.Description;

        return result.IsSuccess
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadRequestProfilesAsync(CancellationToken cancellationToken)
    {
        var tasResult = await _taService.GetAvailableTAsAsync(cancellationToken);
        ViewBag.TAs = tasResult.IsSuccess ? tasResult.Value : Enumerable.Empty<Contracts.TA.TAResponse>();

        var doctorsResult = await _doctorService.GetAvailableDoctorsAsync(cancellationToken);
        ViewBag.Doctors = doctorsResult.IsSuccess ? doctorsResult.Value : Enumerable.Empty<Contracts.Doctor.DoctorResponse>();

        var usersResult = await _userService.GetAllUsersAsync(cancellationToken);
        ViewBag.Students = usersResult.IsSuccess
            ? usersResult.Value.Where(u => u.Role == DefaultRoles.Student && !u.IsDisabled)
            : Enumerable.Empty<Contracts.User.UserResponse>();
    }
}
