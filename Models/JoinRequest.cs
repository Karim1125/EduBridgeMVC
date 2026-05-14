namespace EduBridgeMVC.Models;

public class JoinRequest : BaseRequest
{
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;
    public string? RespondedById { get; set; }
    public ApplicationUser? RespondedBy { get; set; }
}