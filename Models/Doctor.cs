namespace EduBridgeMVC.Models;

public class Doctor : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string Department { get; set; } = string.Empty;
    public string? AcademicTitle { get; set; }
    public string? OfficeLocation { get; set; }

    public int MaxTeams { get; set; }
    public int AvailableTeams { get; set; }

    public bool IsAvailable => AvailableTeams > 0;

    // Navigation
    public ICollection<Team> Teams { get; set; } = [];
    public ICollection<DoctorRequest> DoctorRequests { get; set; } = [];
}