namespace CreateInvoiceSystem.Invoices.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;

public class DeleteInvoiceCommand : CommandBase<Invoice, InvoiceDto>
{
    public override async Task<InvoiceDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(context)); 

        var invoiceEntity = await context.Set<Invoice>()
            //.Include(c => c.Invoice) 
            .FirstOrDefaultAsync(a => a.InvoiceId == Parametr.InvoiceId, cancellationToken: cancellationToken) ??
                              throw new InvalidOperationException($"Invoice with ID {Parametr.InvoiceId} not found.");

        var InvoiceDto = InvoiceMappers.ToDto(invoiceEntity);        
        
        context.Set<Invoice>().Remove(invoiceEntity);
        await context.SaveChangesAsync(cancellationToken);

        return InvoiceDto;
    }
}