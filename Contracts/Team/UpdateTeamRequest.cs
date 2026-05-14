namespace EduBridgeMVC.Contracts.Team;

public record UpdateTeamRequest(
    string? Name,
    string? Description
);