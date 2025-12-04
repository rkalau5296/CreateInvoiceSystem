namespace CreateInvoiceSystem.Clients.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

public class GetClientsQuery : QueryBase<List<Client>>
{
    public override async Task<List<Client>> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Set<Client>()
            .Include(c => c.Address)
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken: cancellationToken) 
            ?? throw new InvalidOperationException($"List of clients is empty.");
    }
}
