using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.DeleteInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
public class DeleteInvoiceHandler(IInvoiceRepository _invoiceRepository) : IRequestHandler<DeleteInvoiceRequest, DeleteInvoiceResponse>
{
    public async Task<DeleteInvoiceResponse> Handle(DeleteInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetInvoiceByIdAsync(request.UserId, request.Id, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Invoice with ID {request.Id} not found.");

        if (invoice.InvoicePositions is { Count: > 0 })
        {
            await _invoiceRepository.RemoveRangeAsync(invoice.InvoicePositions, cancellationToken);
        }

        await _invoiceRepository.RemoveAsync(invoice, cancellationToken);
                
        return new DeleteInvoiceResponse()
        {
            Data = InvoiceMappers.ToDto(invoice)
        };
    }
}

