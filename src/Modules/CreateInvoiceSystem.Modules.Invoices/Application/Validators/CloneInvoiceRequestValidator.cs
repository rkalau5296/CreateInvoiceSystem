using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.CloneInvoice;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Invoices.Application.Validators;

public class CloneInvoiceRequestValidator : AbstractValidator<CloneInvoiceRequest>
{
    public CloneInvoiceRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Id must be greater than or equal to 1.");
    }
}
