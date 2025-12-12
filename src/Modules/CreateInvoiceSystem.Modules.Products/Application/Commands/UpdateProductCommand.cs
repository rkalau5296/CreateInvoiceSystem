namespace CreateInvoiceSystem.Modules.Products.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Products.Dto;
using CreateInvoiceSystem.Modules.Products.Entities;
using CreateInvoiceSystem.Modules.Products.Services;
using Microsoft.EntityFrameworkCore;

public class UpdateProductCommand(IInvoicePositionReadService invoicePosRead) : CommandBase<UpdateProductDto, UpdateProductDto>
{
    public override async Task<UpdateProductDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var product = await context.Set<Product>()            
            .FirstOrDefaultAsync(c => c.ProductId == Parametr.ProductId, cancellationToken: cancellationToken)            
            ?? throw new InvalidOperationException($"Product with ID {Parametr.ProductId} not found.");

        var isUsed = await invoicePosRead.IsProductUsedAsync(Parametr.ProductId, cancellationToken);        

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
