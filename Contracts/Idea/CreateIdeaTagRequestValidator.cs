using FluentValidation;

namespace EduBridgeMVC.Contracts.Idea;

public class CreateIdeaTagRequestValidator : AbstractValidator<CreateIdeaTagRequest>
{
    public CreateIdeaTagRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required")
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required");
    }
}