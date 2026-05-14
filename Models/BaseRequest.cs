using EduBridgeMVC.Abstractions.Consts;

namespace EduBridgeMVC.Models;

public abstract class BaseRequest : AuditableEntity
{
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public string? Message { get; set; }
    public string? ResponseMessage { get; set; }
    public DateTime? RespondedAt { get; set; }
}