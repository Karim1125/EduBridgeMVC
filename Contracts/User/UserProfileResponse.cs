namespace EduBridgeMVC.Contracts.User;

public record UserProfileResponse(
    string FirstName,
    string LastName,
    string? Bio,
    string? Major,
    string? University,
    string? ProfileImageUrl,
    string? GitHubUrl,
    string? LinkedInUrl,
    IEnumerable<string> Skills
);