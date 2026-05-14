namespace EduBridgeMVC.Contracts.Team;

public record CreateTeamRequest(
    string Name,
    string? Description
);