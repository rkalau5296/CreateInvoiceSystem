namespace CreateInvoiceSystem.Address.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Address.Application.DTO;
using CreateInvoiceSystem.Address.Application.Mappers;
using Microsoft.EntityFrameworkCore;

public class UpdateAddressCommand : CommandBase<AddressDto, AddressDto>
{
    public override async Task<AddressDto> Execute(ICreateInvoiceSystemDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(this.Parametr));        

        var existingAddress = await context.Set<AddressDto>().FirstOrDefaultAsync(a => a.AddressId == Parametr.AddressId, cancellationToken: cancellationToken) ?? 
                              throw new InvalidOperationException($"Address with ID {Parametr.AddressId} not found.");

        var entity = AddressMappers.ToEntity(existingAddress);

        entity.Street = Parametr.Street;
        entity.Number = Parametr.Number;
        entity.City = Parametr.City;
        entity.PostalCode = Parametr.PostalCode;

        await context.SaveChangesAsync(cancellationToken);
        return existingAddress;
    }
}
