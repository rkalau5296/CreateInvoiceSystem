using System;
using System.Linq;
using CreateInvoiceSystem.Abstractions.DecimalHelper;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.CreateInvoice;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Validators;

public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    private static readonly string[] NonTaxableRates = ["zw", "np"];

    public CreateInvoiceRequestValidator()
    {
        RuleFor(x => x.Invoice)
            .NotNull()
            .WithMessage("Invoice data cannot be null.");

        When(x => x.Invoice != null, () =>
        {
            RuleFor(x => x.Invoice.Title)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Invoice.ClientId)
                .GreaterThan(0).WithMessage("ClientId is required.");

            RuleFor(x => x.Invoice.CreatedDate)
                .NotEmpty().WithMessage("CreatedDate is required.")
                .Must(date => date.Date <= DateTime.UtcNow.Date)
                .WithMessage("CreatedDate cannot be in the future.");

            RuleFor(x => x.Invoice.PaymentDate)
                .NotEmpty().WithMessage("PaymentDate is required.")
                .GreaterThanOrEqualTo(x => x.Invoice.CreatedDate)
                .WithMessage("PaymentDate cannot be earlier than CreatedDate.");

            RuleFor(x => x.Invoice.Comments)
                .MaximumLength(500).WithMessage("Comments can have maximum 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Invoice.Comments));

            RuleFor(p => p.Invoice.TotalNet)
                .GreaterThan(0).WithMessage("TotalNet is required and must be greater than 0.")
                .Must(v => DecimalHelper.GetDecimalPlaces(v) <= 2)
                .WithMessage("Value must be a decimal with max 2 digits after the decimal point.");

            RuleFor(p => p.Invoice.TotalVat)
                .Must(v => DecimalHelper.GetDecimalPlaces(v) <= 2)
                .WithMessage("Value must be a decimal with max 2 digits after the decimal point.");

            When(x => x.Invoice.InvoicePositions != null &&
                      x.Invoice.InvoicePositions.Any(pos => pos?.VatRate != null &&
                          !NonTaxableRates.Contains(pos.VatRate.Trim().ToLowerInvariant())), () =>
                          {
                              RuleFor(p => p.Invoice.TotalVat)
                                  .GreaterThanOrEqualTo(0).WithMessage("TotalVat is required when there are taxable items.");
                          });

            RuleFor(p => p.Invoice.TotalGross)
                .GreaterThan(0).WithMessage("TotalGross is required and must be greater than 0.")
                .Must(v => DecimalHelper.GetDecimalPlaces(v) <= 2)
                .WithMessage("Value must be a decimal with max 2 digits after the decimal point.");

            RuleFor(x => x.Invoice.MethodOfPayment)
                .NotEmpty().WithMessage("MethodOfPayment is required.")
                .MaximumLength(10).WithMessage("MethodOfPayment can have maximum 10 characters.");
        });
    }
}