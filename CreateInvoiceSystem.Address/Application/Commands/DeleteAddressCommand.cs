namespace CreateInvoiceSystem.Address.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Address.Application.DTO;
using CreateInvoiceSystem.Address.Domain.Entities;
using CreateInvoiceSystem.Address.Application.Mappers;
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
