namespace CreateInvoiceSystem.Products.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;

public class DeleteProductCommand : CommandBase<Product, ProductDto>
{
    public override async Task<ProductDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(context)); 

        var productEntity = await context.Set<Product>()            
            .FirstOrDefaultAsync(a => a.ProductId == Parametr.ProductId, cancellationToken: cancellationToken) ??
                              throw new InvalidOperationException($"Product with ID {Parametr.ProductId} not found.");

        var productDto = ProductMappers.ToDto(productEntity);


        bool isUsed = await context.Set<InvoicePosition>()
            .AnyAsync(i => i.ProductId == Parametr.ProductId, cancellationToken);

        if (!isUsed)
        {            
            context.Set<Product>().Remove(productEntity);            
        }
        else
        {
            productEntity.IsDeleted = true;
            context.Set<Product>().Update(productEntity);
        }
        
        await context.SaveChangesAsync(cancellationToken);

        return productDto;
    }
}