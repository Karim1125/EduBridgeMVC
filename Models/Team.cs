using EduBridgeMVC.Abstractions.Consts;

namespace EduBridgeMVC.Models;

public class Team : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string LeaderId { get; set; } = string.Empty;
    public ApplicationUser Leader { get; set; } = null!;
    public int MaxMembers { get; set; }
    public TeamStatus Status { get; set; } = TeamStatus.Open;
    public Guid? IdeaId { get; set; }
    public Idea? Idea { get; set; }
    public Guid? TaId { get; set; }
    public TeachingAssistant? Ta { get; set; }

    // Navigation
    public ICollection<TeamMember> Members { get; set; } = [];
    public ICollection<JoinRequest> JoinRequests { get; set; } = [];
    public ICollection<TaRequest> TARequests { get; set; } = [];
    public Guid? DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
}