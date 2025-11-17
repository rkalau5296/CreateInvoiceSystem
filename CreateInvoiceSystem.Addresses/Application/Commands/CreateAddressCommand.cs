namespace CreateInvoiceSystem.Addresses.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Addresses.Application.DTO;
using CreateInvoiceSystem.Addresses.Application.Mappers;
using CreateInvoiceSystem.Addresses.Domain.Entities;

public class CreateAddressCommand : CommandBase<AddressDto, AddressDto>
{
    public override async Task<AddressDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(this.Parametr));

        var entity = AddressMappers.ToEntity(this.Parametr);

        await context.Set<Address>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        this.Parametr = AddressMappers.ToDto(entity);
        return this.Parametr;
    }
}
