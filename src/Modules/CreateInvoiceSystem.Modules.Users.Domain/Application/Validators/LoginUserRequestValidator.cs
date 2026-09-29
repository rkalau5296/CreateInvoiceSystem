using FluentValidation;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.LoginUser;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;

public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage("Dane logowania są wymagane.");

        When(x => x.Dto != null, () =>
        {
            RuleFor(x => x.Dto.Email)
                .NotEmpty()
                .WithMessage("Adres e-mail jest wymagany.")
                .EmailAddress()
                .WithMessage("Podano nieprawidłowy format adresu e-mail.");

            RuleFor(x => x.Dto.Password)
                .NotEmpty()
                .WithMessage("Hasło jest wymagane.");
        });
    }
}