namespace CreateInvoiceSystem.Modules.Products.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Products.Dto;
using CreateInvoiceSystem.Modules.Products.Entities;
using CreateInvoiceSystem.Modules.Products.Mappers;
using CreateInvoiceSystem.Modules.Products.Services;
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

        context.Set<Product>().Remove(productEntity);

        await context.SaveChangesAsync(cancellationToken);

        return productDto;
    }
}