namespace EduBridgeMVC.Models;

public class Rating : AuditableEntity
{
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public Guid TaId { get; set; }
    public TeachingAssistant Ta { get; set; } = null!;
    public int Score { get; set; }
    public string? Feedback { get; set; }
}