namespace EduBridgeMVC.Models;

public class TaRequest : BaseRequest
{
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public Guid TAId { get; set; }
    public TeachingAssistant TA { get; set; } = null!;
    public Guid? RespondedByTAId { get; set; }
    public TeachingAssistant? RespondedByTA { get; set; }
}