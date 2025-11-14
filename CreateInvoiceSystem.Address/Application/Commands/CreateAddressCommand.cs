namespace CreateInvoiceSystem.Address.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Address.Application.DTO;
using CreateInvoiceSystem.Address.Application.Mappers;
using CreateInvoiceSystem.Address.Domain.Entities;

public class CreateAddressCommand : CommandBase<AddressDto, AddressDto>
{
    public override async Task<AddressDto> Execute(ICreateInvoiceSystemDbContext context)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(this.Parametr));

        var entity = AddressMappers.ToEntity(this.Parametr);

        await context.Set<Address>().AddAsync(entity);
        await context.SaveChangesAsync();

        this.Parametr = AddressMappers.ToDto(entity);
        return this.Parametr;
    }
}
