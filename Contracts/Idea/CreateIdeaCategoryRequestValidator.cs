using FluentValidation;

namespace EduBridgeMVC.Contracts.Idea;

public class CreateIdeaCategoryRequestValidator : AbstractValidator<CreateIdeaCategoryRequest>
{
    public CreateIdeaCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters");
    }
}