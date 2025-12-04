namespace CreateInvoiceSystem.Invoices.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

public class GetInvoicesQuery : QueryBase<List<Invoice>>
{
    public override async Task<List<Invoice>> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Set<Invoice>()
            .Include(x => x.InvoicePositions)
                .ThenInclude(ip => ip.Product)
            .ToListAsync(cancellationToken: cancellationToken) 
            ?? throw new InvalidOperationException($"List of addresses is empty.");
    }
}
