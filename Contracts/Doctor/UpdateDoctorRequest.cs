using System.ComponentModel.DataAnnotations;

namespace EduBridgeMVC.Contracts.Doctor;

public record UpdateDoctorRequest(
    [Required(ErrorMessage = "Department is required")]
    [MaxLength(100, ErrorMessage = "Department must not exceed 100 characters")]
    string Department,

    [MaxLength(100, ErrorMessage = "Academic title must not exceed 100 characters")]
    string? AcademicTitle,

    [MaxLength(200, ErrorMessage = "Office location must not exceed 200 characters")]
    string? OfficeLocation,

    [Required(ErrorMessage = "Max teams is required")]
    [Range(1, 20, ErrorMessage = "Max teams must be between 1 and 20")]
    int MaxTeams,

    [Range(0, 20, ErrorMessage = "Available teams must be between 0 and 20")]
    int AvailableTeams
);