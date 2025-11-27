namespace CreateInvoiceSystem.Invoices.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
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
        invoice.Value = Parametr.Value;
        invoice.PaymentDate = Parametr.PaymentDate;
        invoice.CreatedDate = Parametr.CreatedDate;
        invoice.Comments = Parametr.Comments;
        invoice.ClientId = Parametr.ClientId;
        invoice.UserId = Parametr.UserId;
        invoice.ProductId = Parametr.ProductId;


        await context.SaveChangesAsync(cancellationToken);        
        return InvoiceMappers.ToDto(invoice);
    }
}
