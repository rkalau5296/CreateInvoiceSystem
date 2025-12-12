namespace CreateInvoiceSystem.Modules.Invoices.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Invoices.Dto;
using CreateInvoiceSystem.Modules.Invoices.Entities;
using CreateInvoiceSystem.Modules.Invoices.Mappers;
using Microsoft.EntityFrameworkCore;

public class UpdateInvoiceCommand : CommandBase<InvoiceDto, InvoiceDto>
{
    public override async Task<InvoiceDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var invoice = await context.Set<Invoice>()            
            .FirstOrDefaultAsync(c => c.InvoiceId == Parametr.InvoiceId, cancellationToken: cancellationToken)            
            ?? throw new InvalidOperationException($"Invoice with ID {Parametr.InvoiceId} not found.");        

        invoice.Title = Parametr.Title;        
        invoice.PaymentDate = Parametr.PaymentDate;
        invoice.CreatedDate = Parametr.CreatedDate;
        invoice.Comments = Parametr.Comments;
        invoice.ClientId = Parametr.ClientId;
        invoice.UserId = Parametr.UserId;        


        await context.SaveChangesAsync(cancellationToken);        
        return InvoiceMappers.ToDto(invoice);
    }
}
