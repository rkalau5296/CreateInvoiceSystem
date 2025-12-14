namespace CreateInvoiceSystem.Modules.Invoices.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Invoices.Dto;
using CreateInvoiceSystem.Modules.Invoices.Entities;
using CreateInvoiceSystem.Modules.Invoices.Mappers;
using Microsoft.EntityFrameworkCore;

public class DeleteInvoiceCommand : CommandBase<Invoice, InvoiceDto>
{
    public override async Task<InvoiceDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(context)); 

        var invoiceEntity = await context.Set<Invoice>()            
            .FirstOrDefaultAsync(a => a.InvoiceId == Parametr.InvoiceId, cancellationToken: cancellationToken) ??
                              throw new InvalidOperationException($"Invoice with ID {Parametr.InvoiceId} not found.");

        var invoiceDto = InvoiceMappers.ToDto(invoiceEntity);        
        
        context.Set<Invoice>().Remove(invoiceEntity);
        await context.SaveChangesAsync(cancellationToken);

        return invoiceDto;
    }
}