namespace EduBridgeMVC.Contracts.Doctor;

public record DoctorResponse(
    Guid Id,
    string UserId,
    string FullName,
    string Email,
    string? ProfileImageUrl,
    string? GitHubUrl,
    string? LinkedInUrl,
    string Department,
    string? AcademicTitle,
    string? OfficeLocation,
    int MaxTeams,
    int AvailableTeams,
    bool IsAvailable
);