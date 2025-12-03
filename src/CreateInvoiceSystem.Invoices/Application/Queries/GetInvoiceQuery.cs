namespace CreateInvoiceSystem.Invoices.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

public class GetInvoiceQuery(int id) : QueryBase<Invoice>
{
    public override async Task<Invoice> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Set<Invoice>()
            .Include(x => x.InvoicePositions)
                .ThenInclude(ip => ip.Product)
            .FirstOrDefaultAsync(c => c.InvoiceId == id, cancellationToken: cancellationToken) 
            ?? throw new InvalidOperationException($"Invoice with ID {id} not found.");
    }
}