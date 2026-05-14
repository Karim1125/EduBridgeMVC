namespace EduBridgeMVC.Contracts.Authentication;

public record ConfirmEmailRequest(
    string UserId,
    string Code,
    string? Role = null
);