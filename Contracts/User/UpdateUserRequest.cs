using System.ComponentModel.DataAnnotations;

namespace EduBridgeMVC.Contracts.User;

public record UpdateUserRequest(
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    string? FirstName,

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    string? LastName,

    string? Bio,
    string? Major,
    string? University,
    string? GitHubUrl,
    string? LinkedInUrl
);