using FluentValidation;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ResendToken;

namespace CreateInvoiceSystem.Modules.Users.Application.Validators;

public class ResendActivationTokenRequestValidator : AbstractValidator<ResendActivationTokenRequest>
{
    public ResendActivationTokenRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany.")
            .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
            .WithMessage("Nieprawidłowy format adresu email.");
    }
}