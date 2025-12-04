using CreateInvoiceSystem.Abstractions.Entities;

namespace CreateInvoiceSystem.Abstractions.Dto;

public record InvoicePositionDto
(
    int InvoicePositionId,
    int InvoiceId,    
    int? ProductId,
    ProductDto Product,
    string Description,
    string Name,
    decimal? Value,
    int Quantity
);
