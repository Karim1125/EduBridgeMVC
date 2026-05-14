using System.ComponentModel.DataAnnotations;

namespace EduBridgeMVC.Contracts.TA;

public record CreateTaRequest(
    [Required(ErrorMessage = "Department is required")]
    [MaxLength(100, ErrorMessage = "Department must not exceed 100 characters")]
    string Department,

    [MaxLength(100, ErrorMessage = "Academic title must not exceed 100 characters")]
    string? AcademicTitle,

    [MaxLength(200, ErrorMessage = "Office location must not exceed 200 characters")]
    string? OfficeLocation,

    [Required(ErrorMessage = "Max slots is required")]
    [Range(1, 20, ErrorMessage = "Max slots must be between 1 and 20")]
    int MaxSlots
);
