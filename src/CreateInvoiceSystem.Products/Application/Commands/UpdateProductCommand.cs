namespace CreateInvoiceSystem.Products.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;

public class UpdateProductCommand : CommandBase<ProductDto, ProductDto>
{
    public override async Task<ProductDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var Product = await context.Set<Product>()            
            .FirstOrDefaultAsync(c => c.ProductId == Parametr.ProductId, cancellationToken: cancellationToken)            
            ?? throw new InvalidOperationException($"Product with ID {Parametr.ProductId} not found.");        

        Product.Name = Parametr.Name;
        Product.Value = Parametr.Value;

        await context.SaveChangesAsync(cancellationToken);        
        return ProductMappers.ToDto(Product);
    }
}
