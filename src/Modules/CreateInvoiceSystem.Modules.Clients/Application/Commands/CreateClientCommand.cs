using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Clients.Dto;
using CreateInvoiceSystem.Modules.Clients.Entities;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Clients.Application.Commands;

public class CreateClientCommand : CommandBase<CreateClientDto, CreateClientDto>
{
    public override async Task<CreateClientDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));
        if (this.Parametr.Address is null)
            throw new ArgumentNullException(nameof(this.Parametr.Address));

        var exists = await context.Set<Client>()
            

            .AnyAsync(c =>
                c.Name == this.Parametr.Name &&
                c.Address != null &&
                c.Address.Street == this.Parametr.Address.Street &&
                c.Address.Number == this.Parametr.Address.Number &&
                c.Address.City == this.Parametr.Address.City &&
                c.Address.PostalCode == this.Parametr.Address.PostalCode &&
                c.Address.Country == this.Parametr.Address.Country &&
                c.UserId == this.Parametr.UserId,
                cancellationToken);

        if (exists)
            throw new InvalidOperationException("A client with the same name and address already exists.");

        var entity = ClientMappers.ToEntity(this.Parametr);

        await context.Set<Client>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return this.Parametr;
    }
}