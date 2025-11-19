using CreateInvoiceSystem.Addresses.Application.RequestsResponses.CreateAddress;
using FluentValidation;

namespace CreateInvoiceSystem.Addresses.Application.Validators;

public class UpdateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
{
    public UpdateAddressRequestValidator()
    {
        RuleFor(x => x.Address)
            .NotNull().WithMessage("Address cannot be null.");

        When(x => x.Address != null, () =>
        {
            RuleFor(x => x.Address.Street)
                .NotEmpty().WithMessage("Street is required.")
                .MaximumLength(100).WithMessage("Street cannot exceed 100 characters.");

            RuleFor(x => x.Address.Number)
                .NotEmpty().WithMessage("Number cannot be empty.");

            RuleFor(x => x.Address.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(50).WithMessage("City cannot exceed 50 characters.");

            RuleFor(x => x.Address.PostalCode)
                .NotEmpty().WithMessage("PostalCode is required.")
                .Matches(@"^\d{2}-\d{3}$").WithMessage("PostalCode must be in a valid format.");

            RuleFor(x => x.Address.Country)
                .NotEmpty().WithMessage("Country is required.")
                .MaximumLength(50).WithMessage("Country cannot exceed 50 characters.");

            RuleFor(x => x.Address.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Matches(@"^[\w\.-]+@[\w\.-]+\.[A-Za-z]{2,}$").WithMessage("Email must be in a valid format.");
        });
    }
}
