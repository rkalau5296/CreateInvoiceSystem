using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Products.Dto;
using CreateInvoiceSystem.Modules.Products.Entities;
using CreateInvoiceSystem.Modules.Products.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Products.Application.Commands;

public class UpdateProductCommand : CommandBase<UpdateProductDto, UpdateProductDto>
{
    public override async Task<UpdateProductDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var product = await context.Set<Product>()
            .FirstOrDefaultAsync(c => c.ProductId == Parametr.ProductId, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {Parametr.ProductId} not found.");

        var oldName = product.Name;
        var oldDescription = product.Description;
        var oldValue = product.Value;

        product.Name = this.Parametr.Name ?? product.Name;
        product.Description = this.Parametr.Description ?? product.Description;
        product.Value = this.Parametr.Value ?? product.Value;

        await context.SaveChangesAsync(cancellationToken);
        var persisted = await context.Set<Product>()
        .AsNoTracking()
        .SingleOrDefaultAsync(p => p.ProductId == product.ProductId, cancellationToken);

        bool hasChanged = persisted is not null && (
            !string.Equals(oldName, persisted.Name, StringComparison.Ordinal) ||
            !string.Equals(oldDescription, persisted.Description, StringComparison.Ordinal) ||
            oldValue != persisted.Value
        );

        return hasChanged
            ? ProductMappers.ToUpdatedDto(persisted)
            : throw new InvalidOperationException($"No new data for product with ID {Parametr.ProductId}.");
    }
}
