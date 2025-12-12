using CreateInvoiceSystem.Modules.Products.Dto;

namespace CreateInvoiceSystem.Modules.InvoicePositions.Dto;

public record InvoicePositionDto
(
    int InvoicePositionId,
    int InvoiceId,    
    int? ProductId,
    ProductDto Product,
    string ProductName,
    string ProductDescription,
    decimal? ProductValue,
    int Quantity
);