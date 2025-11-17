namespace CreateInvoiceSystem.Addresses.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Addresses.Application.DTO;
using CreateInvoiceSystem.Addresses.Domain.Entities;
using CreateInvoiceSystem.Addresses.Application.Mappers;
using Microsoft.EntityFrameworkCore;

public class DeleteAddressCommand : CommandBase<Address, AddressDto>
{
    public override async Task<AddressDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(Parametr));

        var addressEntity = await context.Set<Address>().FirstOrDefaultAsync(a => a.AddressId == Parametr.AddressId, cancellationToken: cancellationToken) ??
                              throw new InvalidOperationException($"Address with ID {Parametr.AddressId} not found.");

        var addressDto = AddressMappers.ToDto(addressEntity);

        context.Set<Address>().Remove(addressEntity);
        await context.SaveChangesAsync(cancellationToken);

        return addressDto;
    }
}
