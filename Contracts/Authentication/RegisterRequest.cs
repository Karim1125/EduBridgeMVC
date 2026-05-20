using System.ComponentModel.DataAnnotations;

namespace EduBridgeMVC.Contracts.Authentication;

public record RegisterRequest(
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    string FirstName,

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    string LastName,

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    string Password,

    [Required(ErrorMessage = "Please confirm your password")]
    //[Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    string ConfirmPassword,

    string? Bio,
    string? Major,
    string? University,

    [Required(ErrorMessage = "Please select a role")]
    string Role,

    string? GitHubUrl,
    string? LinkedInUrl,
    string? SecurityCode,
    string? PersistedProfileImageDataUrl,
    IFormFile? ProfileImage
);
