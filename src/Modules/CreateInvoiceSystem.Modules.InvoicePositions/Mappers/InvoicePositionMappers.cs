using CreateInvoiceSystem.Modules.InvoicePositions.Dto;
using CreateInvoiceSystem.Modules.InvoicePositions.Entities;
using CreateInvoiceSystem.Modules.Products.Dto;
using CreateInvoiceSystem.Modules.Products.Mappers;

namespace CreateInvoiceSystem.Modules.InvoicePositions.Mappers;

public static class InvoicePositionMappers
{
    public static InvoicePositionDto ToDto(this InvoicePosition invoicePosition) =>
    invoicePosition == null
        ? throw new ArgumentNullException(nameof(invoicePosition))
        : new(            
            invoicePosition.InvoicePositionId,
            invoicePosition.InvoiceId,
            invoicePosition.ProductId,
            invoicePosition.Product != null
                ? new ProductDto(
                    invoicePosition.Product.ProductId,
                    invoicePosition.Product.Name,
                    invoicePosition.Product.Description,
                    invoicePosition.Product.Value,
                    invoicePosition.Product.UserId,
                    invoicePosition.Product.IsDeleted
                  )
                : null,
            invoicePosition.ProductName,
            invoicePosition.ProductDescription,
            invoicePosition.ProductValue,
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
            Quantity = dto.Quantity
        };   
    
}
