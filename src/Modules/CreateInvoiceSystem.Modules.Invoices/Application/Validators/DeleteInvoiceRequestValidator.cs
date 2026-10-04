using FluentValidation;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.DeleteInvoice;

namespace CreateInvoiceSystem.Modules.Invoices.Application.Validators;

public class DeleteInvoiceRequestValidator : AbstractValidator<DeleteInvoiceRequest>
{
    public DeleteInvoiceRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Invoice Id must be greater than 0.");        
    }
}