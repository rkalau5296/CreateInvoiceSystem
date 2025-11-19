namespace CreateInvoiceSystem.Addresses.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Addresses.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class GetAddressesQuery : QueryBase<List<Address>>
{
    public override async Task<List<Address>> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Set<Address>().ToListAsync(cancellationToken: cancellationToken) 
            ?? throw new InvalidOperationException($"List of addresses is empty.");
    }
}
