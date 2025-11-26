namespace CreateInvoiceSystem.Clients.Application.Validators;

using CreateInvoiceSystem.Clients.Application.RequestsResponses.UpdateClient;
using FluentValidation;

public class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequest>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(x => x.Client.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Client.Nip)
            .NotEmpty().WithMessage("Nip number is required.")
            .Matches(@"^\d{10}$")
            .WithMessage("The Nip number must contain exactly 10 digits.");
        //RuleFor(x => x.UserId)
        //    .GreaterThan(0).WithMessage("UserId is required.");

        RuleFor(x => x.Client.AddressDto)
            .NotNull().WithMessage("Address must be specified.");
        
        When(x => x.Client.AddressDto != null, () =>
        {
            RuleFor(x => x.Client.AddressDto.Street)
                .NotEmpty().WithMessage("Street is required in address.");

            RuleFor(x => x.Client.AddressDto.Number)
                .NotEmpty().WithMessage("Street number is required in address.");

            RuleFor(x => x.Client.AddressDto.City)
                .NotEmpty().WithMessage("City is required in address.");

            RuleFor(x => x.Client.AddressDto.PostalCode)
                .NotEmpty().WithMessage("Postal code is required in address.");

            RuleFor(p => p.Client.AddressDto.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
                .WithMessage("Invalid email address.");

            RuleFor(x => x.Client.AddressDto.Country)
                .NotEmpty().WithMessage("Postal code is required in address.");
        });
    }
}