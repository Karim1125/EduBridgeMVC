using FluentValidation;

namespace EduBridgeMVC.Contracts.TA;

public class SendTaRequestRequestValidator : AbstractValidator<SendTaRequestRequest>
{
    public SendTaRequestRequestValidator()
    {
        RuleFor(x => x.TAId)
            .NotEmpty().WithMessage("TA ID is required");

        RuleFor(x => x.Message)
            .Must(message => !string.IsNullOrWhiteSpace(message)).WithMessage("Message cannot be empty")
            .MaximumLength(1000).WithMessage("Message cannot exceed 1000 characters")
            .When(x => x.Message is not null);
    }
}