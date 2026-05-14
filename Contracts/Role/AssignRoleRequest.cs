namespace EduBridgeMVC.Contracts.Role;

public record AssignRoleRequest(
    string UserId,
    string RoleName
);