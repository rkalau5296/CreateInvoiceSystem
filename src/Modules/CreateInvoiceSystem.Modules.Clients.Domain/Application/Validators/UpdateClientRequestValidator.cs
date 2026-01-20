using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.UpdateClient;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Validators;
public class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequest>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(x => x.Client.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100)
            .When(x => x.Client.Name != null);

        RuleFor(x => x.Client.Nip)
            .NotEmpty().WithMessage("Nip number is required.")
            .Matches(@"^\d{10}$")
            .WithMessage("The Nip number must contain exactly 10 digits.")
            .When(x => x.Client.Name != null);        
        
        When(x => x.Client.Address != null, () =>
        {
            RuleFor(x => x.Client.Address.Street)
                .NotEmpty().WithMessage("Street is required in address.")
                .When(x => x.Client.Name != null);

            RuleFor(x => x.Client.Address.Number)
                .NotEmpty().WithMessage("Street number is required in address.")
                .When(x => x.Client.Name != null);

            RuleFor(x => x.Client.Address.City)
                .NotEmpty().WithMessage("City is required in addess.")
                .When(x => x.Client.Name != null);

            RuleFor(x => x.Client.Address.PostalCode)
                .NotEmpty().WithMessage("Postal code is required in address.")
                .Matches(@"^\d{2}-\d{3}$")
                .WithMessage("Postal code must be in the format XX-XXX.")
                .When(x => x.Client.Name != null);

            RuleFor(x => x.Client.Address.Country)
                .NotEmpty().WithMessage("Postal code is required in address.")
                .When(x => x.Client.Name != null);
        });
    }
}