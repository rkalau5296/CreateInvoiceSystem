namespace CreateInvoiceSystem.Addresses.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Addresses.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class GetAddressQuery(int id) : QueryBase<Address>
{
    public override async Task<Address> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Set<Address>().FirstOrDefaultAsync(a => a.AddressId == id, cancellationToken: cancellationToken) 
            ?? throw new InvalidOperationException($"Address with ID {id} not found.");
    }
}