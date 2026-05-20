using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduBridgeMVC.Controllers;

[Authorize]
[Route("[controller]")]
public class SearchController(
    IUserService userService,
    ITaService taService,
    IDoctorService doctorService) : Controller
{
    private readonly IUserService _userService = userService;
    private readonly ITaService _taService = taService;
    private readonly IDoctorService _doctorService = doctorService;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, string? type, CancellationToken cancellationToken)
    {
        var query = q?.Trim() ?? string.Empty;
        var searchType = NormalizeType(type);
        var results = string.IsNullOrWhiteSpace(query)
            ? []
            : await SearchByNameAsync(query, searchType, cancellationToken);

        var model = new SearchPageViewModel(query, searchType, results);
        return View(model);
    }

    private async Task<IReadOnlyList<SearchResultViewModel>> SearchByNameAsync(
        string query,
        string type,
        CancellationToken cancellationToken)
    {
        var results = new List<SearchResultViewModel>();

        if (type is "all" or "ta")
        {
            var tas = await _taService.GetAllTAsAsync(cancellationToken);
            if (tas.IsSuccess)
            {
                results.AddRange(tas.Value
                    .Where(ta => MatchesName(ta.FullName, query))
                    .Select(ta => new SearchResultViewModel(
                        ta.Id.ToString(),
                        ta.FullName,
                        "TA",
                        ta.Email,
                        ta.Department,
                        ta.ProfileImageUrl,
                        "Ta")));
            }
        }

        if (type is "all" or "doctor")
        {
            var doctors = await _doctorService.GetAllAsync(cancellationToken);
            if (doctors.IsSuccess)
            {
                results.AddRange(doctors.Value
                    .Where(doctor => MatchesName(doctor.FullName, query))
                    .Select(doctor => new SearchResultViewModel(
                        doctor.Id.ToString(),
                        doctor.FullName,
                        "Doctor",
                        doctor.Email,
                        doctor.Department,
                        doctor.ProfileImageUrl,
                        "Doctor")));
            }
        }

        if (type is "all" or "student")
        {
            var users = await _userService.GetAllUsersAsync(cancellationToken);
            if (users.IsSuccess)
            {
                results.AddRange(users.Value
                    .Where(user => user.Role == DefaultRoles.Student)
                    .Where(user => MatchesName($"{user.FirstName} {user.LastName}", query))
                    .Select(user => new SearchResultViewModel(
                        user.Id,
                        $"{user.FirstName} {user.LastName}",
                        "Student",
                        user.Email,
                        null,
                        user.ProfileImageUrl,
                        "User")));
            }
        }

        return results
            .OrderBy(result => result.Name)
            .ToList();
    }

    private static bool MatchesName(string name, string query) =>
        name.Contains(query, StringComparison.OrdinalIgnoreCase);

    private static string NormalizeType(string? type) =>
        type?.Trim().ToLowerInvariant() switch
        {
            "ta" => "ta",
            "doctor" => "doctor",
            "student" => "student",
            _ => "all"
        };
}

public record SearchPageViewModel(
    string Query,
    string Type,
    IReadOnlyList<SearchResultViewModel> Results);

public record SearchResultViewModel(
    string Id,
    string Name,
    string Type,
    string Email,
    string? Department,
    string? ProfileImageUrl,
    string ControllerName);
