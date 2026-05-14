namespace EduBridgeMVC.Models;

public class Skill : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<UserSkill> Users { get; set; } = [];
}