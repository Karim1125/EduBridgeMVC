using EduBridgeMVC.Abstractions;

namespace EduBridgeMVC.Errors;

public static class IdeaErrors
{
    public static readonly Error IdeaNotFound = new(
        "Idea.NotFound", "Idea not found", StatusCodes.Status404NotFound);

    public static readonly Error TeamAlreadyHasIdea = new(
        "Idea.TeamAlreadyHasIdea", "This team already has an idea", StatusCodes.Status409Conflict);
}