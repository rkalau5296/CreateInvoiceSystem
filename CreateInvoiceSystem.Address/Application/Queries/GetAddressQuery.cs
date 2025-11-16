namespace CreateInvoiceSystem.Address.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Address.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class GetAddressQuery(int id) : QueryBase<Address>
{
    public int Id { get; set; } = id;

    public override async Task<Address> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Set<Address>().FirstOrDefaultAsync(a => a.AddressId == Id, cancellationToken: cancellationToken);
    }
}
