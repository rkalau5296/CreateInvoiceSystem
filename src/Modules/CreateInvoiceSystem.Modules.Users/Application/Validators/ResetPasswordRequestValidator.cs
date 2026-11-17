using FluentValidation;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ResetPassword;

namespace CreateInvoiceSystem.Modules.Users.Application.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany.")
            .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
            .WithMessage("Nieprawidłowy format adresu email.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token jest wymagany.");

        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Wersja jest wymagana.")
            .Must(version => int.TryParse(version, out var parsedVersion) && parsedVersion >= 0)
            .WithMessage("Wersja musi być poprawną liczbą większą lub równą 0.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Nowe hasło jest wymagane.");
    }
}