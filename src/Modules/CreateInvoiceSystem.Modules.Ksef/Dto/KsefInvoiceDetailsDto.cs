namespace CreateInvoiceSystem.Modules.Ksef.Dto;

public sealed record KsefInvoiceDetailsDto(
    string KsefNumber,
    string InvoiceNumber,
    DateOnly? IssueDate,
    DateOnly? SaleDate,
    string Currency,
    KsefInvoicePartyDto Seller,
    KsefInvoicePartyDto Buyer,
    IReadOnlyList<KsefInvoiceLineDto> Lines,
    decimal NetAmount,
    decimal VatAmount,
    decimal GrossAmount,
    DateOnly? PaymentDueDate,
    string? PaymentMethod,
    string? BankAccount);

public sealed record KsefInvoicePartyDto(
    string? Name,
    string? Nip,
    string? Address,
    string? Email);

public sealed record KsefInvoiceLineDto(
    int Number,
    string? Description,
    string? UnitOfMeasure,
    decimal Quantity,
    decimal UnitPrice,
    decimal NetAmount,
    string? VatRate);
