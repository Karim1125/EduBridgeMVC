using FluentValidation;

namespace EduBridgeMVC.Contracts.Idea;

public class UpdateIdeaRequestValidator : AbstractValidator<UpdateIdeaRequest>
{
    public UpdateIdeaRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description cannot be empty")
            .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.RepositoryUrl)
            .Must(BeAValidUrl).WithMessage("Invalid repository URL format")
            .When(x => x.RepositoryUrl is not null);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID cannot be empty")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.Tags)
            .Must(tags => tags!.All(tag => !string.IsNullOrWhiteSpace(tag)))
            .WithMessage("Tags cannot contain empty values")
            .Must(tags => tags!.All(tag => tag.Length <= 50)).WithMessage("Each tag cannot exceed 50 characters")
            .When(x => x.Tags is not null);
    }

    private static bool BeAValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}