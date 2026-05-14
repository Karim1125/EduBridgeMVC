namespace EduBridgeMVC.Contracts.User;

public record UserResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string? ProfileImageUrl,
    string? GitHubUrl,
    string? LinkedInUrl,
    DateTime CreatedAt,
    bool IsDisabled
);