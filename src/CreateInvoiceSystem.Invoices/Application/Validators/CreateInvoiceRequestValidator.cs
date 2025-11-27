namespace CreateInvoiceSystem.Invoices.Application.Validators;

using CreateInvoiceSystem.Invoices.Application.RequestsResponses.CreateInvoice;
using CreateInvoiceSystem.Abstractions.DecimalHelper;
using FluentValidation;


public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceRequestValidator()
    {
        RuleFor(x => x.Invoice.Title)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);
        RuleFor(x => x.Invoice.ClientId)
            .GreaterThan(0).WithMessage("ClientId is required.");

        RuleFor(x => x.Invoice.ProductId)
            .GreaterThan(0).WithMessage("ProductId is required.");

        RuleFor(x => x.Invoice.CreatedDate)
            .NotEmpty().WithMessage("CreatedDate is required.")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("CreatedDate cannot be in the future.");

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

        RuleFor(p => p.Invoice.Value)
            .NotEmpty().WithMessage("Value is required.")
            .GreaterThanOrEqualTo(0)
            .Must(v => DecimalHelper.GetDecimalPlaces(v) <= 2)
            .WithMessage("Value must be a decimal with max 2 digits after the decimal point.");
    }
}