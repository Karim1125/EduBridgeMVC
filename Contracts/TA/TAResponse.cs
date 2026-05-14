namespace EduBridgeMVC.Contracts.TA;

public record TAResponse(
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
    int MaxSlots,
    int AvailableSlots,
    bool IsAvailable,
    double AverageRating
);