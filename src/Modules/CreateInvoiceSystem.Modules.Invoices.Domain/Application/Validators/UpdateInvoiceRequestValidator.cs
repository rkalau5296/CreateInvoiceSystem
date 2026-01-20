using CreateInvoiceSystem.Abstractions.DecimalHelper;
using FluentValidation;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.UpdateInvoice;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Validators;
public class UpdateInvoiceRequestValidator : AbstractValidator<UpdateInvoiceRequest>
{
    public UpdateInvoiceRequestValidator()
    {
        RuleFor(x => x.Invoice.Title)
           .NotEmpty().WithMessage("Name is required.")
           .MaximumLength(100);
        RuleFor(x => x.Invoice.ClientId)
            .GreaterThan(0).WithMessage("ClientId is required.");

        RuleFor(x => x.Invoice.CreatedDate)
            .NotEmpty().WithMessage("CreatedDate is required.");            

        RuleFor(x => x.Invoice.PaymentDate)
            .NotEmpty().WithMessage("PaymentDate is required.")
            .GreaterThanOrEqualTo(x => x.Invoice.CreatedDate).WithMessage("PaymentDate cannot be earlier than CreatedDate.");

        RuleFor(x => x.Invoice.CreatedDate)
            .NotEmpty().WithMessage("CreatedDate is required.")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("CreatedDate cannot be in the future.");

        RuleFor(x => x.Invoice.Comments)
            .MaximumLength(500).WithMessage("Comments can have maximum 500 characters.");

        RuleFor(x => x.Invoice.UserId)
            .GreaterThan(0).WithMessage("UserId is required.");

        RuleFor(p => p.Invoice.TotalAmount)
            .NotEmpty().WithMessage("Value is required.")
            .GreaterThanOrEqualTo(0)
            .Must(v => DecimalHelper.GetDecimalPlaces(v) <= 2)
            .WithMessage("Value must be a decimal with max 2 digits after the decimal point.");

        RuleFor(x => x.Invoice.MethodOfPayment)
            .NotEmpty().WithMessage("MethodOfPayment is required.")
            .MaximumLength(10).WithMessage("MethodOfPayment can have maximum 10 characters.");

        //RuleFor(x => x.Invoice.Product)
        //    .NotEmpty().WithMessage("Product is required.")
        //    .MaximumLength(100).WithMessage("Product can have maximum 10 characters.");
    }    
}