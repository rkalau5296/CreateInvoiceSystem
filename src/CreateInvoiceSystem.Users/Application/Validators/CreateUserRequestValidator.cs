namespace CreateInvoiceSystem.Users.Application.Validators;

using CreateInvoiceSystem.Users.Application.RequestsResponses;
using CreateInvoiceSystem.Users.Application.RequestsResponses.CreateUser;
using FluentValidation;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.User.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(x => x.User.CompanyName)
           .NotEmpty().WithMessage("CompanyName is required.")
           .MaximumLength(100);

        RuleFor(p => p.User.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
            .WithMessage("Invalid email address.");

        RuleFor(x => x.User.Nip)            
            .Matches(@"^\d{10}$")
            .WithMessage("The Nip number must contain exactly 10 digits.");

        RuleFor(x => x.User.Address)
            .NotNull().WithMessage("Address must be specified.");

        When(x => x.User.Address != null, () =>
        {
            RuleFor(x => x.User.Address.Street)
                .NotEmpty().WithMessage("Street is required in address.");

            RuleFor(x => x.User.Address.Number)
                .NotEmpty().WithMessage("Street number is required in address.");

            RuleFor(x => x.User.Address.City)
                .NotEmpty().WithMessage("City is required in address.");

            RuleFor(x => x.User.Address.PostalCode)
                .NotEmpty().WithMessage("Postal code is required in address.")
                .Matches(@"^\d{2}-\d{3}$")
                .WithMessage("Postal code must be in the format XX-XXX.");
            
            RuleFor(x => x.User.Address.Country)
                .NotEmpty().WithMessage("Postal code is required in address.");
        });

    }
}