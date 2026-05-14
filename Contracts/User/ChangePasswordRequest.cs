using System.ComponentModel.DataAnnotations;

namespace EduBridgeMVC.Contracts.User;

public record ChangePasswordRequest(
    [Required(ErrorMessage = "Current password is required")]
    string CurrentPassword,

    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    string NewPassword,

    [Required(ErrorMessage = "Please confirm your new password")]
  //  [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    string ConfirmNewPassword
);
