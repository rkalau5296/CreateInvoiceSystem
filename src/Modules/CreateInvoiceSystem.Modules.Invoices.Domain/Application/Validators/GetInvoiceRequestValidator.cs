using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoice;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Validators;

public class GetInvoiceRequestValidator : AbstractValidator<GetInvoiceRequest>
{
    public GetInvoiceRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Id must be greater than or equal to 1.");
    }
}