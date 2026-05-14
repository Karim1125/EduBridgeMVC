namespace EduBridgeMVC.Contracts.Role;

public record RoleResponse(
    string Id,
    string Name,
    bool IsDefault
);