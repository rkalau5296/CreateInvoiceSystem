namespace CreateInvoiceSystem.Modules.Products.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Products.Entities;
using Microsoft.EntityFrameworkCore;

public class GetProductsQuery : QueryBase<List<Product>>
{
    public override async Task<List<Product>> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Set<Product>()
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken: cancellationToken) 
            ?? throw new InvalidOperationException($"List of addresses is empty.");
    }
}
