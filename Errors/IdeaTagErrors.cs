using EduBridgeMVC.Abstractions;

namespace EduBridgeMVC.Errors;

public static class IdeaTagErrors
{
    public static readonly Error TagNotFound = new(
        "IdeaTag.NotFound", "Tag not found", StatusCodes.Status404NotFound);

    public static readonly Error DuplicateTagName = new(
        "IdeaTag.AlreadyExists", "A tag with the same name already exists in this category",
        StatusCodes.Status409Conflict);
}