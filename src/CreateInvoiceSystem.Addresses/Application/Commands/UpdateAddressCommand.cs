namespace CreateInvoiceSystem.Addresses.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.DTO;
using CreateInvoiceSystem.Addresses.Application.Mappers;
using CreateInvoiceSystem.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

public class UpdateAddressCommand : CommandBase<AddressDto, AddressDto>
{
    public override async Task<AddressDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var address = await context.Set<Address>().FirstOrDefaultAsync(a => a.AddressId == Parametr.AddressId, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Address with ID {Parametr.AddressId} not found.");   

        address.Street = this.Parametr.Street;
        address.Number = this.Parametr.Number;
        address.City = this.Parametr.City;
        address.PostalCode = this.Parametr.PostalCode;

        await context.SaveChangesAsync(cancellationToken);        
        return AddressMappers.ToDto(address);
    }
}
