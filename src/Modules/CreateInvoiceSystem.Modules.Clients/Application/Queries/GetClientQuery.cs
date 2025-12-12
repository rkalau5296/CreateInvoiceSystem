using CreateInvoiceSystem.Modules.Clients.Entities;

namespace CreateInvoiceSystem.Modules.Clients.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using Microsoft.EntityFrameworkCore;

public class GetClientQuery(int id) : QueryBase<Client>
{
    public override async Task<Client> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Set<Client>()
            .Include(c => c.Address)
            .Where(c => !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.ClientId == id, cancellationToken: cancellationToken) 
            ?? throw new InvalidOperationException($"Client with ID {id} not found.");
    }
}