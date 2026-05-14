using EduBridgeMVC.Abstractions.Consts;
using FluentValidation;

namespace EduBridgeMVC.Contracts.Authentication;

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Token is required");

        RuleFor(x => x.Role)
            .Must(role => string.IsNullOrWhiteSpace(role) ||
                          role is DefaultRoles.Student or DefaultRoles.TA or DefaultRoles.Doctor)
            .WithMessage("Invalid role");
    }
}