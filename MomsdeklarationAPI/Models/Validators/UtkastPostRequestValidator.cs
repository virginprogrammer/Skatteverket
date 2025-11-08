using FluentValidation;
using MomsdeklarationAPI.Models.Requests;

namespace MomsdeklarationAPI.Models.Validators;

public class UtkastPostRequestValidator : AbstractValidator<UtkastPostRequest>
{
    public UtkastPostRequestValidator()
    {
        RuleFor(x => x.Momsuppgift)
            .NotNull()
            .WithMessage("Momsuppgift is required")
            .SetValidator(new MomsuppgiftValidator());

        RuleFor(x => x.Kommentar)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Kommentar))
            .WithMessage("Comment cannot exceed 500 characters");
    }
}