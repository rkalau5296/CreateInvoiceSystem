namespace CreateInvoiceSystem.Address.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Address.Application.DTO;
using Microsoft.EntityFrameworkCore;

public class GetAddressesQuery : QueryBase<List<AddressDto>>
{
    public override async Task<List<AddressDto>> Execute(IDbContext context)
    {
        return await context.Set<AddressDto>().ToListAsync();
    }
}
