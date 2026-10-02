namespace CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
public record UpdateInvoicePositionDto
(
    int InvoicePositionId,
    int InvoiceId,
    int? ProductId, 
    string ProductName,
    string? ProductDescription,
    decimal? ProductValue,
    int Quantity,
    string VatRate,
    ProductDto? Product
);
