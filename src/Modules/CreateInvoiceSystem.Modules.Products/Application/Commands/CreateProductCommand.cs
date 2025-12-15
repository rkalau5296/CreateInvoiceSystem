namespace CreateInvoiceSystem.Modules.Products.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Products.Dto;
using CreateInvoiceSystem.Modules.Products.Entities;
using CreateInvoiceSystem.Modules.Products.Mappers;
using Microsoft.EntityFrameworkCore;

public class CreateProductCommand : CommandBase<CreateProductDto, CreateProductDto>
{
    public override async Task<CreateProductDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var exists = await context.Set<Product>()
        .AsNoTracking()
        .AnyAsync(p =>
            p.Name == this.Parametr.Name &&
            p.UserId == this.Parametr.UserId,
            cancellationToken);

        if (exists)
            throw new InvalidOperationException("The product with the same name is already exists.");

        var entity = ProductMappers.ToEntity(this.Parametr);

        await context.Set<Product>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var persisted = await context.Set<Product>()
            .AsNoTracking()            
            .SingleOrDefaultAsync(c => c.ProductId == entity.ProductId, cancellationToken);

        return persisted is not null
            ? ProductMappers.ToCreateDto(entity)
            : throw new InvalidOperationException("Product was saved but could not be reloaded.");
    }
}
