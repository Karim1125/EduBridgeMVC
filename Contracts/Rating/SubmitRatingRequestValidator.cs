using EduBridgeMVC.Errors;
using FluentValidation;

namespace EduBridgeMVC.Contracts.Rating;

public class SubmitRatingRequestValidator : AbstractValidator<SubmitRatingRequest>
{
    public SubmitRatingRequestValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(1, 100)
            .WithMessage(RatingErrors.InvalidScore.Description)
            .WithErrorCode(RatingErrors.InvalidScore.Code);
    }
}