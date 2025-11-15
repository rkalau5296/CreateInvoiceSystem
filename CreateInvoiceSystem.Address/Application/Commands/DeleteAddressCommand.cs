namespace CreateInvoiceSystem.Address.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Address.Application.DTO;
using CreateInvoiceSystem.Address.Domain.Entities;
using CreateInvoiceSystem.Address.Application.Mappers;
using Microsoft.EntityFrameworkCore;


public class DeleteAddressCommand : CommandBase<AddressDto, AddressDto>
{
    public override async Task<AddressDto> Execute(ICreateInvoiceSystemDbContext context, CancellationToken cancellationToken = default)
    {
        var existingAddress = await context.Set<AddressDto>().FirstOrDefaultAsync(a => a.AddressId == Parametr.AddressId) ??
                              throw new InvalidOperationException($"Address with ID {Parametr.AddressId} not found.");

        var entity = AddressMappers.ToEntity(existingAddress);

        context.Set<Address>().Remove(entity);
        await context.SaveChangesAsync();

        return existingAddress;
    }
}
