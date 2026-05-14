namespace EduBridgeMVC.Models;

public class TeachingAssistant : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string Department { get; set; } = string.Empty;
    public string? AcademicTitle { get; set; }
    public string? OfficeLocation { get; set; }
    public int MaxSlots { get; set; }
    public int AvailableSlots { get; set; }
    public bool IsAvailable => AvailableSlots > 0;
}