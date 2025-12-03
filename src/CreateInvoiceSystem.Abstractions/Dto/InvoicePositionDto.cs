using CreateInvoiceSystem.Abstractions.Entities;

namespace CreateInvoiceSystem.Abstractions.Dto;

public record InvoicePositionDto
(
    int InvoicePositionId,
    int InvoiceId,
    //Invoice Invoice,
    int? ProductId,
    ProductDto ProductDto,
    string Description,
    string Name,
    decimal? Value,
    int Quantity
);
