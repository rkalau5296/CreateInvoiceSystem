namespace CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
public record InvoicePositionDto
(
    int InvoicePositionId,
    int InvoiceId,    
    int? ProductId,
    ProductDto Product,
    string ProductName,
    string ProductDescription,
    decimal? ProductValue,
    int Quantity,
    string VatRate
);