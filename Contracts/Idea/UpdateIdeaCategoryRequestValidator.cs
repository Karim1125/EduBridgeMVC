using FluentValidation;

namespace EduBridgeMVC.Contracts.Idea;

public class UpdateIdeaCategoryRequestValidator : AbstractValidator<UpdateIdeaCategoryRequest>
{
    public UpdateIdeaCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters");
    }
}