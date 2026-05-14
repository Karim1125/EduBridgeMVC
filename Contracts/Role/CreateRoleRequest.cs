namespace EduBridgeMVC.Contracts.Role;

public record CreateRoleRequest(
    string Name,
    bool IsDefault
);