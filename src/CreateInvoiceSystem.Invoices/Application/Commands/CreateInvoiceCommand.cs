namespace CreateInvoiceSystem.Invoices.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;

public class CreateInvoiceCommand : CommandBase<InvoiceDto, InvoiceDto>
{
    public override async Task<InvoiceDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var entity = InvoiceMappers.ToEntity(this.Parametr);

        await context.Set<Invoice>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        this.Parametr = InvoiceMappers.ToDto(entity);
        return this.Parametr;
    }
}
