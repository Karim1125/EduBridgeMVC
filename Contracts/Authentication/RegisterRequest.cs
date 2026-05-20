using System.ComponentModel.DataAnnotations;

namespace EduBridgeMVC.Contracts.Authentication;

public record RegisterRequest
{
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; init; } = string.Empty;

    public string? Bio { get; init; }
    public string? Major { get; init; }
    public string? University { get; init; }

    [Required(ErrorMessage = "Please select a role")]
    public string Role { get; init; } = string.Empty;

    public string? GitHubUrl { get; init; }
    public string? LinkedInUrl { get; init; }
    public string? SecurityCode { get; init; }
    public IFormFile? ProfileImage { get; init; }
    public string? PersistedProfileImageDataUrl { get; init; }
}
