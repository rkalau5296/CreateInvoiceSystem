using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;
public class DeleteInvoiceCommand : CommandBase<Invoice, InvoiceDto, IInvoiceRepository>
{
    public override async Task<InvoiceDto> Execute(IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(this.Parametr));        

        var invoiceEntity = await _invoiceRepository.GetInvoiceByIdAsync(
            Parametr.UserId,
            Parametr.InvoiceId,            
            cancellationToken) 
            ?? throw new InvalidOperationException($"Invoice with ID {Parametr.InvoiceId} not found.");        

        if (invoiceEntity.InvoicePositions is not null && invoiceEntity.InvoicePositions.Any())
        {
            await _invoiceRepository.RemoveRangeAsync(invoiceEntity.InvoicePositions, cancellationToken);
        }

        await _invoiceRepository.RemoveAsync(invoiceEntity);
        await _invoiceRepository.SaveChangesAsync(cancellationToken);

        bool invExists = await _invoiceRepository.InvoiceExistsAsync(Parametr.InvoiceId, cancellationToken);
        bool posExists = await _invoiceRepository.InvoicePositionExistsAsync(Parametr.InvoiceId, cancellationToken);

        return !invExists && !posExists
            ? InvoiceMappers.ToDto(invoiceEntity)
            : throw new InvalidOperationException($"Failed to delete Invoice or InvoicePosition with ID {Parametr.InvoiceId}.");
    }
}