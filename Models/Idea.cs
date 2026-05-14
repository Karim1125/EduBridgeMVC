namespace EduBridgeMVC.Models;

public class IdeaCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<IdeaTag> Tags { get; set; } = [];
    public ICollection<Idea> Ideas { get; set; } = [];
}

public class IdeaTag : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public IdeaCategory Category { get; set; } = null!;

    // Navigation
    public ICollection<Idea> Ideas { get; set; } = [];
}

public class Idea : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? RepositoryUrl { get; set; }
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public IdeaCategory Category { get; set; } = null!;

    // Navigation
    public ICollection<IdeaTag> Tags { get; set; } = [];
}