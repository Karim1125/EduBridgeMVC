using EduBridgeMVC.Abstractions;

namespace EduBridgeMVC.Errors;

public static class SkillErrors
{
    public static readonly Error SkillNotFound = new(
        "Skill.NotFound",
        "The skill with the specified ID was not found or has been deleted.",
        StatusCodes.Status404NotFound);

    public static readonly Error DuplicateSkillName = new(
        "Skill.DuplicateName",
        "A skill with the same name already exists.",
        StatusCodes.Status409Conflict);
}