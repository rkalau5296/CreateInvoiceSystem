namespace CreateInvoiceSystem.Abstractions.Mappers;

using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;

public static class InvoicePositionMappers
{    
    public static InvoicePositionDto ToDto(this InvoicePosition invoicePosition) =>
        invoicePosition == null
        ? throw new ArgumentNullException(nameof(invoicePosition), "InvoicePosition cannot be null when mapping to InvoicePositionDto.")
        :
        new(
            invoicePosition.InvoicePositionId, 
            invoicePosition.InvoiceId, 
            invoicePosition.ProductId, 
            invoicePosition.Product?.ToDto(), 
            invoicePosition.Description, 
            invoicePosition.Name, 
            invoicePosition.Value, 
            invoicePosition.Quantity
            );

    public static InvoicePosition ToEntity(this InvoicePositionDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "InvoicePositionDto cannot be null when mapping to InvoicePosition.")
        :
        new()
        {
            InvoicePositionId = dto.InvoicePositionId,
            InvoiceId = dto.InvoiceId,            
            ProductId = dto.ProductId,
            Product = dto.Product.ToEntity(),
            Description = dto.Description,
            Name = dto.Name,
            Value = dto.Value,
            Quantity = dto.Quantity
        };

    public static InvoicePosition MapToInvoicePosition(InvoicePositionDto dto) =>
        dto == null
        ? throw new ArgumentNullException(nameof(dto), "InvoicePositionDto cannot be null when mapping to InvoicePosition.")
        :
         new() 
        {
            ProductId = dto.ProductId,
            Name = dto.Name,
            Description = dto.Description,
            Value = dto.Value,
            Quantity = dto.Quantity           
        };
    
    public static List<InvoicePositionDto> ToDtoList(this IEnumerable<InvoicePosition> invoicePositions) =>
        invoicePositions == null
        ? throw new ArgumentNullException(nameof(invoicePositions), "InvoicePositions lis cannot be null when mapping to InvoicePositionDto's list.")
        :
         [.. invoicePositions.Select(a => a.ToDto())];
}
