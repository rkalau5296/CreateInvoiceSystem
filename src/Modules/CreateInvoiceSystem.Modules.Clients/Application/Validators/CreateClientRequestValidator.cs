namespace CreateInvoiceSystem.Modules.Clients.Application.Validators;

using Clients.Application.RequestsResponses.CreateClient;
using FluentValidation;


public class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(x => x.Client.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Client.Nip)
            .NotEmpty().WithMessage("Nip number is required.")
            .Matches(@"^\d{10}$")
            .WithMessage("The Nip number must contain exactly 10 digits.");       

        RuleFor(x => x.Client.Address)
            .NotNull().WithMessage("Address must be specified.");
        
        When(x => x.Client.Address != null, () =>
        {
            RuleFor(x => x.Client.Address.Street)
                .NotEmpty().WithMessage("Street is required in address.");

            RuleFor(x => x.Client.Address.Number)
                .NotEmpty().WithMessage("Street number is required in address.");

            RuleFor(x => x.Client.Address.City)
                .NotEmpty().WithMessage("City is required in address.");

            RuleFor(x => x.Client.Address.PostalCode)
                .NotEmpty().WithMessage("Postal code is required in address.")
                .Matches(@"^\d{2}-\d{3}$")
                .WithMessage("Postal code must be in the format XX-XXX.");
            
            RuleFor(x => x.Client.Address.Country)
                .NotEmpty().WithMessage("Postal code is required in address.");
        });
    }
}