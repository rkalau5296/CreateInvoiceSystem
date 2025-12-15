namespace CreateInvoiceSystem.Modules.Invoices.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using CreateInvoiceSystem.Modules.InvoicePositions.Entities;
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
            .Include(i => i.InvoicePositions)
            .FirstOrDefaultAsync(a => a.InvoiceId == Parametr.InvoiceId, cancellationToken: cancellationToken) ??
                              throw new InvalidOperationException($"Invoice with ID {Parametr.InvoiceId} not found.");

        var invoiceDto = InvoiceMappers.ToDto(invoiceEntity);

        if (invoiceEntity.InvoicePositions is not null && invoiceEntity.InvoicePositions.Count > 0)
        {
            context.Set<InvoicePosition>().RemoveRange(invoiceEntity.InvoicePositions);
        }

        context.Set<Invoice>().Remove(invoiceEntity);
        await context.SaveChangesAsync(cancellationToken);

        bool invExists = await context.Set<Invoice>()
            .AsNoTracking()
            .AnyAsync(i => i.InvoiceId == Parametr.InvoiceId, cancellationToken);       

        bool posExists = await context.Set<InvoicePosition>()
            .AsNoTracking()
            .AnyAsync(p => p.InvoiceId == Parametr.InvoiceId, cancellationToken);


        return (!invExists && !posExists)
            ? InvoiceMappers.ToDto(invoiceEntity)
            : throw new InvalidOperationException($"Failed to delete Invoice or InvoicePosition with ID {Parametr.InvoiceId}.");
    }
}