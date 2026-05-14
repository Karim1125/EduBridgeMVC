using EduBridgeMVC.Abstractions;

namespace EduBridgeMVC.Errors;

public static class IdeaCategoryErrors
{
    public static readonly Error CategoryNotFound = new(
        "IdeaCategory.NotFound",
        "The category with the specified ID was not found or has been deleted.",
        StatusCodes.Status404NotFound);

    public static readonly Error DuplicateCategoryName = new(
        "IdeaCategory.DuplicateName",
        "A category with the same name already exists.",
        StatusCodes.Status409Conflict);
}