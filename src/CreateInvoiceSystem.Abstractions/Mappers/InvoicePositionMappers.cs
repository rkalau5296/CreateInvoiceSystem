namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class InvoicePositionMappers
{    
    public static InvoicePositionDto ToDto(this InvoicePosition invoicePosition) =>
        new(invoicePosition.InvoicePositionId, invoicePosition.InvoiceId, invoicePosition.Invoice, invoicePosition.ProductId, invoicePosition.Product, invoicePosition.Name, invoicePosition.Description, invoicePosition.Quantity, invoicePosition.UnitPrice);
    public static InvoicePosition ToEntity(this InvoicePositionDto dto) =>
        new()
        {
            InvoicePositionId = dto.InvoicePositionId,
            InvoiceId = dto.InvoiceId,
            Invoice = dto.Invoice,
            ProductId = dto.ProductId,
            Product = dto.Product,
            Name = dto.Name,
            Description = dto.Description,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice
        };

    public static List<InvoicePositionDto> ToDtoList(this IEnumerable<InvoicePosition> invoicePositions) =>
         [.. invoicePositions.Select(a => a.ToDto())];
}
