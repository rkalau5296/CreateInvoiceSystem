using FluentValidation;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ActivateUser;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;

public class ActivateUserRequestValidator : AbstractValidator<ActivateUserRequest>
{
    public ActivateUserRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Błąd: Brak tokena aktywacyjnego.");
    }
}