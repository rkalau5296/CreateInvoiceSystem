using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetPdf;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Invoices.Application.Validators;

public class GetInvoicePdfRequestValidator : AbstractValidator<GetInvoicePdfRequest>
{
    public GetInvoicePdfRequestValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Invoice ID must be greater than or equal to 1.");       
    }
}