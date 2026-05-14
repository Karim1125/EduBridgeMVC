using Microsoft.AspNetCore.Identity;

namespace EduBridgeMVC.Models;

public class ApplicationUser : IdentityUser
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid().ToString();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Major { get; set; }
    public string? University { get; set; }
    public string? ProfileImageUrl { get; set; }

    public string? GitHubUrl { get; set; }

    public string? LinkedInUrl { get; set; }
    public bool IsDisabled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<UserSkill> Skills { get; set; } = [];
    public ICollection<TeamMember> Teams { get; set; } = [];
    public TeachingAssistant? TeachingAssistant { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}