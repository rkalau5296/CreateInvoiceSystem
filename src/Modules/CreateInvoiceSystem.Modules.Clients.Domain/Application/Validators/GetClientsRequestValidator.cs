using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClients;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Validators;

public class GetClientsRequestValidator : AbstractValidator<GetClientsRequest>
{
    public GetClientsRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 1000)
            .WithMessage("Page size must be between 1 and 1000.");
    }
}