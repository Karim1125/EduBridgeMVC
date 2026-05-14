using EduBridgeMVC.Abstractions.Consts;

namespace EduBridgeMVC.Contracts.Skills;

public record JoinRequestResponse(
    Guid Id,
    Guid TeamId,
    string TeamName,
    string StudentName,
    RequestStatus Status,
    string? Message,
    string? ResponseMessage,
    DateTime CreatedAt,
    DateTime? RespondedAt
);