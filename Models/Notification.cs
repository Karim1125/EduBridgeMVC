using EduBridgeMVC.Abstractions.Consts;

namespace EduBridgeMVC.Models;

public class Notification : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public NotificationType Type { get; set; }
    public Guid? RelatedEntityId { get; set; }
}