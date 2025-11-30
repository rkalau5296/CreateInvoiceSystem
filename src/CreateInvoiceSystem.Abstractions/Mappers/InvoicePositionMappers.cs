namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class InvoicePositionMappers
{    
    public static InvoicePositionDto ToDto(this InvoicePosition invoicePosition) =>
        new(invoicePosition.InvoicePositionId, invoicePosition.InvoiceId, invoicePosition.Invoice, invoicePosition.ProductId, invoicePosition.Product, invoicePosition.Description, invoicePosition.Name, invoicePosition.Value, invoicePosition.Quantity);
    public static InvoicePosition ToEntity(this InvoicePositionDto dto) =>
        new()
        {
            InvoicePositionId = dto.InvoicePositionId,
            InvoiceId = dto.InvoiceId,
            Invoice = dto.Invoice,
            ProductId = dto.ProductId,
            Product = dto.Product,
            Description = dto.Description,
            Name = dto.Name,
            Value = dto.Value,
            Quantity = dto.Quantity
        };

    public static InvoicePosition MapToInvoicePosition(InvoicePositionDto dto)
    {
        return new InvoicePosition
        {
            ProductId = dto.ProductId,
            Name = dto.Name,
            Description = dto.Description,
            Value = dto.Value,
            Quantity = dto.Quantity
            // Product obiekt powiązujesz w serwisie/db gdy ProductId != null
        };
    }
    public static List<InvoicePositionDto> ToDtoList(this IEnumerable<InvoicePosition> invoicePositions) =>
         [.. invoicePositions.Select(a => a.ToDto())];
}
