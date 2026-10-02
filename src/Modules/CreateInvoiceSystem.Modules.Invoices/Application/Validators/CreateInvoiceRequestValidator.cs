using System;
using System.Linq;
using CreateInvoiceSystem.Abstractions.DecimalHelper;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.CreateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Validators;

public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    private static readonly string[] AllowedVatRates = ["23%", "8%", "5%", "0%", "zw", "np"];
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
            
            RuleFor(x => x.Invoice)
                .Must(invoice => invoice.ClientId.HasValue || (invoice.Client != null && !IsClientDtoEmpty(invoice.Client)))
                .WithMessage("Invoice must contain a valid ClientId or Client details.");

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

            RuleFor(x => x.Invoice.InvoicePositions)
                .NotNull().WithMessage("Invoice positions list cannot be null.")
                .Must(pos => pos != null && pos.Count > 0)
                .WithMessage("Invoice must contain at least one position.");

            RuleForEach(x => x.Invoice.InvoicePositions)
                .ChildRules(position =>
                {
                    position.RuleFor(p => p)
                        .Must(p => p.ProductId.HasValue || p.Product != null || !string.IsNullOrWhiteSpace(p.ProductName))
                        .WithMessage("InvoicePosition must contain Product details, ProductId, or ProductName.");

                    position.RuleFor(p => p.Quantity)
                        .GreaterThan(0).WithMessage(p => $"Quantity must be greater than 0 for product: {p.ProductName}");

                    position.RuleFor(p => p.VatRate)
                        .NotEmpty().WithMessage(p => $"VatRate cannot be empty for product: {p.ProductName}")
                        .Must(rate => rate != null && AllowedVatRates.Contains(rate.Trim().ToLowerInvariant()))
                        .WithMessage(p => $"Invalid VatRate: {p.VatRate}. Allowed values are: {string.Join(", ", AllowedVatRates)}");
                });
        });
    }

    private static bool IsClientDtoEmpty(CreateClientDto client)
    {
        if (client == null) return true;

        return string.IsNullOrEmpty(client.Name)
            && string.IsNullOrEmpty(client.Nip)
            && (client.Address == null ||
                (string.IsNullOrEmpty(client.Address.Street) &&
                 string.IsNullOrEmpty(client.Address.Number) &&
                 string.IsNullOrEmpty(client.Address.City) &&
                 string.IsNullOrEmpty(client.Address.PostalCode) &&
                 string.IsNullOrEmpty(client.Address.Country)))
            && string.IsNullOrEmpty(client.Email);
    }
}