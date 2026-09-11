using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;
public class DeleteInvoiceCommand : CommandBase<Invoice, InvoiceDto, IInvoiceRepository>
{
    public override async Task<InvoiceDto> Execute(IInvoiceRepository invoiceRepository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Parametr);

        var invoice = await invoiceRepository.GetInvoiceByIdAsync(Parametr.UserId, Parametr.InvoiceId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Invoice with ID {Parametr.InvoiceId} not found.");

        if (invoice.InvoicePositions is { Count: > 0 })
        {
            await invoiceRepository.RemoveRangeAsync(invoice.InvoicePositions, cancellationToken);
        }

        await invoiceRepository.RemoveAsync(invoice, cancellationToken);

        return InvoiceMappers.ToDto(invoice);
    }
}