namespace CreateInvoiceSystem.Products.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;

public class UpdateProductCommand : CommandBase<UpdateProductDto, UpdateProductDto>
{
    public override async Task<UpdateProductDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var product = await context.Set<Product>()            
            .FirstOrDefaultAsync(c => c.ProductId == Parametr.ProductId, cancellationToken: cancellationToken)            
            ?? throw new InvalidOperationException($"Product with ID {Parametr.ProductId} not found.");

        bool isUsed = await context.Set<InvoicePosition>()
            .AnyAsync(i => i.ProductId == Parametr.ProductId, cancellationToken);

        if (!isUsed)
        {
            product.Name = this.Parametr.Name ?? product.Name;
            product.Description = this.Parametr.Description ?? product.Description;
            product.Value = this.Parametr.Value ?? product.Value;
        }
        else
        {
            throw new InvalidOperationException("The product cannot be edited because it has already been used in documents. Please update data on a new product or create a new client.");
        }

        await context.SaveChangesAsync(cancellationToken);        
        return this. Parametr;
    }
}
