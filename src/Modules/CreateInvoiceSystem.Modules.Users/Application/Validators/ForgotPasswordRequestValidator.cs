using FluentValidation;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ForgotPassword;

namespace CreateInvoiceSystem.Modules.Users.Application.Validators;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage("Dane żądania są wymagane.");

        When(x => x.Dto != null, () =>
        {
            RuleFor(x => x.Dto.Email)
                .NotEmpty()
                .WithMessage("Adres e-mail jest wymagany.")
                .EmailAddress()
                .WithMessage("Podano nieprawidłowy format adresu e-mail.");
        });
    }
}