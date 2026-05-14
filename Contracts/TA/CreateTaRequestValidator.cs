using FluentValidation;

namespace EduBridgeMVC.Contracts.TA;

public class CreateTaRequestValidator : AbstractValidator<CreateTaRequest>
{
    public CreateTaRequestValidator()
    {
        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required")
            .MaximumLength(100).WithMessage("Department must not exceed 100 characters");

        RuleFor(x => x.AcademicTitle)
            .MaximumLength(100).WithMessage("Academic title must not exceed 100 characters")
            .When(x => x.AcademicTitle is not null);

        RuleFor(x => x.OfficeLocation)
            .MaximumLength(200).WithMessage("Office location must not exceed 200 characters")
            .When(x => x.OfficeLocation is not null);

        RuleFor(x => x.MaxSlots)
            .GreaterThan(0).WithMessage("Max slots must be greater than 0")
            .LessThanOrEqualTo(20).WithMessage("Max slots must not exceed 20");
    }
}