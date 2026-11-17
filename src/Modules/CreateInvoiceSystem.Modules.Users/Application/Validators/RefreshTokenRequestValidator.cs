using FluentValidation;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.RefreshToken;

namespace CreateInvoiceSystem.Modules.Users.Application.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Token odświeżania jest wymagany.");
    }
}