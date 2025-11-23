namespace CreateInvoiceSystem.Clients.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

public class GetClientQuery(int id) : QueryBase<Client>
{
    public override async Task<Client> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Set<Client>().FirstOrDefaultAsync(a => a.AddressId == id, cancellationToken: cancellationToken) 
            ?? throw new InvalidOperationException($"Address with ID {id} not found.");
    }
}