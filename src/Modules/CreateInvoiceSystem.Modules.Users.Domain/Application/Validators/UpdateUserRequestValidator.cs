using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.UpdateUser;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.User.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100)
            .When(x => x.User.Name != null);

        RuleFor(x => x.User.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(100)
            .When(x => x.User.CompanyName != null);

        RuleFor(x => x.User.Nip)
           .Matches(@"^\d{10}$")
           .WithMessage("The Nip number must contain exactly 10 digits.")
           .When(x => x.User.Nip != null);

        RuleFor(p => p.User.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
            .WithMessage("Invalid email address.")
            .When(x => x.User.Email != null);        

        When(x => x.User.Address != null, () =>
        {
            RuleFor(x => x.User.Address.Street)
                .NotEmpty().WithMessage("Street is required in address.")
                .When(x => x.User.Address.Street != null);

            RuleFor(x => x.User.Address.Number)
                .NotEmpty().WithMessage("Street number is required in address.")
                .When(x => x.User.Address.Number != null);

            RuleFor(x => x.User.Address.City)
                .NotEmpty().WithMessage("City is required in address.")
                .When(x => x.User.Address.City != null);

            RuleFor(x => x.User.Address.PostalCode)
                .NotEmpty().WithMessage("Postal code is required in address.")
                .Matches(@"^\d{2}-\d{3}$")
                .WithMessage("Postal code must be in the format XX-XXX.")
                .When(x => x.User.Address.PostalCode != null);

            RuleFor(x => x.User.Address.Country)
                .NotEmpty().WithMessage("Country is required in address.")
                .When(x => x.User.Address.Country != null);
        });
    }
}