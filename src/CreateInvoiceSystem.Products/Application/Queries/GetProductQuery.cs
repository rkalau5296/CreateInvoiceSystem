namespace CreateInvoiceSystem.Products.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

public class GetProductQuery(int id) : QueryBase<Product>
{
    public override async Task<Product> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Set<Product>()
            .Where(c => !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.ProductId == id, cancellationToken: cancellationToken) 
            ?? throw new InvalidOperationException($"Product with ID {id} not found.");
    }
}