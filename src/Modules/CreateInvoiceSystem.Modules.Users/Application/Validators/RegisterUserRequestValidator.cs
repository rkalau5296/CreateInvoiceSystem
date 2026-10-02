using FluentValidation;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RegisterUser;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.User)
            .NotNull()
            .WithMessage("User data cannot be null.");

        When(x => x.User != null, () =>
        {
            RuleFor(x => x.User.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100);

            RuleFor(x => x.User.CompanyName)
                .NotEmpty().WithMessage("CompanyName is required.")
                .MaximumLength(100);

            RuleFor(x => x.User.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
                .WithMessage("Invalid email address.");

            RuleFor(x => x.User.Password)
                .NotEmpty().WithMessage("Password is required.");

            RuleFor(x => x.User.Nip)
                .Matches(@"^\d{10}$")
                .WithMessage("The Nip number must contain exactly 10 digits.")
                .When(x => !string.IsNullOrEmpty(x.User.Nip));

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
                    .NotEmpty().WithMessage("Country is required in address.");
            });
        });
    }
}