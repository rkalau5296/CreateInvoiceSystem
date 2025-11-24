namespace CreateInvoiceSystem.Addresses.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.DTO;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;

public class CreateAddressCommand : CommandBase<AddressDto, AddressDto>
{
    public override async Task<AddressDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var entity = AddressMappers.ToEntity(this.Parametr);

        await context.Set<Address>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        this.Parametr = AddressMappers.ToDto(entity);
        return this.Parametr;
    }
}
