namespace EduBridgeMVC.Contracts.TA;

public record SendTaRequestRequest(
    Guid TAId,
    string? Message
);