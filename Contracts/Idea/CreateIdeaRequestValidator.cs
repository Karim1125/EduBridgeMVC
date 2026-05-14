using FluentValidation;

namespace EduBridgeMVC.Contracts.Idea;

public class CreateIdeaRequestValidator : AbstractValidator<CreateIdeaRequest>
{
    public CreateIdeaRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters");

        RuleFor(x => x.RepositoryUrl)
            .Must(BeAValidUrl).WithMessage("Invalid repository URL format")
            .When(x => x.RepositoryUrl is not null);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required");

        RuleFor(x => x.Tags)
            .NotNull().WithMessage("Tags are required")
            .Must(tags => tags.Count > 0).WithMessage("At least one tag is required")
            .Must(tags => tags.All(tag => !string.IsNullOrWhiteSpace(tag))).WithMessage("Tags cannot be empty")
            .Must(tags => tags.All(tag => tag.Length <= 50)).WithMessage("Each tag cannot exceed 50 characters");
    }

    private static bool BeAValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}