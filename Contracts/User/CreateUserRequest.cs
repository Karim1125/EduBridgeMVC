using System.ComponentModel.DataAnnotations;

namespace EduBridgeMVC.Contracts.User;

public record CreateUserRequest(
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
   // [Compare("Password", ErrorMessage = "Passwords do not match")]
    string ConfirmPassword,

    [MaxLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
    string? Bio,

    [MaxLength(100, ErrorMessage = "Major cannot exceed 100 characters")]
    string? Major,

    [MaxLength(200, ErrorMessage = "University cannot exceed 200 characters")]
    string? University,

    [Required(ErrorMessage = "Please select a role")]
    string Role
);
