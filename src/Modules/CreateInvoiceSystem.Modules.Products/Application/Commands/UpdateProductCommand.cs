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


        product.Name = this.Parametr.Name ?? product.Name;
        product.Description = this.Parametr.Description ?? product.Description;
        product.Value = this.Parametr.Value ?? product.Value;


        await context.SaveChangesAsync(cancellationToken);
        return this.Parametr;
    }
}
