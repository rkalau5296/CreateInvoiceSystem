using FluentValidation;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.DeleteInvoice;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Validators;

public class DeleteInvoiceRequestValidator : AbstractValidator<DeleteInvoiceRequest>
{
    public DeleteInvoiceRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Invoice Id must be greater than 0.");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User Id must be greater than 0.");
    }
}