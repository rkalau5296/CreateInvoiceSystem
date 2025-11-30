namespace CreateInvoiceSystem.Invoices.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;
using System.Net;

public class CreateInvoiceCommand : CommandBase<CreateInvoiceDto, CreateInvoiceDto>
{
    public override async Task<CreateInvoiceDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        Invoice entity = null;
        
        if (this.Parametr.ClientId is null)
        {
            var client = ClientMappers.ToEntity(this.Parametr.Client);
            entity = InvoiceMappers.FromCreateInvoiceDtoToInvoiceIfClientIdIsNull(this.Parametr, client);
            await context.Set<Client>().AddAsync(client, cancellationToken);            
        }
        else if( this.Parametr.ClientId is not null)
        {            
            entity = InvoiceMappers.FromCreateInvoiceDtoToInvoiceIfClientIdIsNotNull(this.Parametr);           
        }


        await context.Set<Invoice>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);        
        
        return this.Parametr;
    }
}
