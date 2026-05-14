using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Settings;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace EduBridgeMVC.Contracts.Authentication;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IOptions<SecurityCodeSettings> securitySettings)
    {
        var settings = securitySettings.Value;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters")
            .When(x => x.Bio is not null);

        RuleFor(x => x.Major)
            .MaximumLength(100).WithMessage("Major cannot exceed 100 characters")
            .When(x => x.Major is not null);

        RuleFor(x => x.University)
            .MaximumLength(100).WithMessage("University cannot exceed 100 characters")
            .When(x => x.University is not null);

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(r => r is DefaultRoles.Student or DefaultRoles.TA or DefaultRoles.Doctor)
            .WithMessage("Invalid role");

        RuleFor(x => x.SecurityCode)
            .NotEmpty()
            .WithMessage("A security code is required to register as TA or Doctor")
            .Must((req, code) =>
                req.Role == DefaultRoles.TA
                    ? code == settings.TaCode
                    : code == settings.DoctorCode)
            .WithMessage("Invalid security code for the selected role")
            .When(x => x.Role is DefaultRoles.TA or DefaultRoles.Doctor);

    }
}