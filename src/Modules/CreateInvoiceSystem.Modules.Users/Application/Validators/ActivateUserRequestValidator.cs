using FluentValidation;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ActivateUser;

namespace CreateInvoiceSystem.Modules.Users.Application.Validators;

public class ActivateUserRequestValidator : AbstractValidator<ActivateUserRequest>
{
    public ActivateUserRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Błąd: Brak tokena aktywacyjnego.");
    }
}