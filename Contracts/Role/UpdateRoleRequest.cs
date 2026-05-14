namespace EduBridgeMVC.Contracts.Role;

public record UpdateRoleRequest(
    string Name,
    bool IsDefault
);