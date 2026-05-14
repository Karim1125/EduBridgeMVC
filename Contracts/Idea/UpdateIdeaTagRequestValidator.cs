using FluentValidation;

namespace EduBridgeMVC.Contracts.Idea;

public class UpdateIdeaTagRequestValidator : AbstractValidator<UpdateIdeaTagRequest>
{
    public UpdateIdeaTagRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name cannot be empty")
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters")
            .When(x => x.Name is not null);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID cannot be empty")
            .When(x => x.CategoryId.HasValue);
    }
}