namespace CreateInvoiceSystem.Modules.Products.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Products.Dto;
using CreateInvoiceSystem.Modules.Products.Entities;
using CreateInvoiceSystem.Modules.Products.Mappers;

public class CreateProductCommand : CommandBase<CreateProductDto, CreateProductDto>
{
    public override async Task<CreateProductDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var entity = ProductMappers.ToEntity(this.Parametr);

        await context.Set<Product>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        return this.Parametr;
    }
}
