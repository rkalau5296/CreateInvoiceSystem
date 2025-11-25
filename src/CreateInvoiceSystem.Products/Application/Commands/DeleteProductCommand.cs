namespace CreateInvoiceSystem.Products.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.DTO;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;

public class DeleteProductCommand : CommandBase<Product, ProductDto>
{
    public override async Task<ProductDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(context)); 

        var ProductEntity = await context.Set<Product>()
            //.Include(c => c.Product) 
            .FirstOrDefaultAsync(a => a.ProductId == Parametr.ProductId, cancellationToken: cancellationToken) ??
                              throw new InvalidOperationException($"Product with ID {Parametr.ProductId} not found.");

        var ProductDto = ProductMappers.ToDto(ProductEntity);        
        
        context.Set<Product>().Remove(ProductEntity);
        await context.SaveChangesAsync(cancellationToken);

        return ProductDto;
    }
}