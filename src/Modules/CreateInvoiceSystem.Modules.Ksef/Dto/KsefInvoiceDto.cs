namespace CreateInvoiceSystem.Modules.Ksef.Dto;

public sealed record KsefInvoiceDto(
    string KsefNumber,
    string InvoiceNumber,
    DateTimeOffset IssueDate,
    string? SellerName,
    string? SellerNip,
    string? BuyerName,
    decimal NetAmount,
    decimal VatAmount,
    decimal GrossAmount,
    string Currency);
