using FluentValidation;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ChangePassword;

namespace CreateInvoiceSystem.Modules.Users.Application.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage("Dane zmiany hasła są wymagane.");

        When(x => x.Dto != null, () =>
        {
            RuleFor(x => x.Dto.OldPassword)
                .NotEmpty()
                .WithMessage("Stare hasło jest wymagane.");

            RuleFor(x => x.Dto.NewPassword)
                .NotEmpty()
                .WithMessage("Nowe hasło jest wymagane.")
                .MinimumLength(6)
                .WithMessage("Nowe hasło musi mieć co najmniej 6 znaków.");

            RuleFor(x => x.Dto.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Potwierdzenie hasła jest wymagane.")
                .Equal(x => x.Dto.NewPassword)
                .WithMessage("Nowe hasło i potwierdzenie nie są zgodne.");
        });
    }
}