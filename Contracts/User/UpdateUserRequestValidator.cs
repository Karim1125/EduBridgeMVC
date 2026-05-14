using FluentValidation;

namespace EduBridgeMVC.Contracts.User;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
            .When(x => x.LastName is not null);

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters")
            .When(x => x.Bio is not null);

        RuleFor(x => x.Major)
            .MaximumLength(100).WithMessage("Major cannot exceed 100 characters")
            .When(x => x.Major is not null);

        RuleFor(x => x.University)
            .MaximumLength(100).WithMessage("University cannot exceed 100 characters")
            .When(x => x.University is not null);
    }
}