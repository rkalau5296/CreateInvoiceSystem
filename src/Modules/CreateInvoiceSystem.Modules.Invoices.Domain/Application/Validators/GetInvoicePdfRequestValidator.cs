using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetPdf;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Validators;

public class GetInvoicePdfRequestValidator : AbstractValidator<GetInvoicePdfRequest>
{
    public GetInvoicePdfRequestValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Invoice ID must be greater than or equal to 1.");       
    }
}