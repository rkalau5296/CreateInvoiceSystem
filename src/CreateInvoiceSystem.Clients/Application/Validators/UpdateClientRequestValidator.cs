namespace CreateInvoiceSystem.Clients.Application.Validators;

using FluentValidation;
using CreateInvoiceSystem.Abstractions.Entities;

public class UpdateClientRequestValidator : AbstractValidator<Client>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(x => x.AddressId)
            .GreaterThan(0).WithMessage("Address is required.");

        //RuleFor(x => x.UserId)
        //    .GreaterThan(0).WithMessage("UserId is required.");

        RuleFor(x => x.Address)
            .NotNull().WithMessage("Address must be specified.");
        
        When(x => x.Address != null, () =>
        {
            RuleFor(x => x.Address.Street)
                .NotEmpty().WithMessage("Street is required in address.");

            RuleFor(x => x.Address.Number)
                .NotEmpty().WithMessage("Street number is required in address.");

            RuleFor(x => x.Address.City)
                .NotEmpty().WithMessage("City is required in address.");

            RuleFor(x => x.Address.PostalCode)
                .NotEmpty().WithMessage("Postal code is required in address.");

            RuleFor(x => x.Address.Country)
                .NotEmpty().WithMessage("Postal code is required in address.");
        });
    }
}