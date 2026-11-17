using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetInvoice;
using CreateInvoiceSystem.Modules.Invoices.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Application.Handlers;

public class GetInvoiceHandler(IInvoiceRepository _invoiceRepository) : IRequestHandler<GetInvoiceRequest, GetInvoiceResponse>
{
    public async Task<GetInvoiceResponse> Handle(GetInvoiceRequest request, CancellationToken cancellationToken)
    {        
        var invoice = await _invoiceRepository.GetInvoiceByIdAsync(request.UserId, request.Id, cancellationToken)
               ?? throw new InvalidOperationException($"Invoice with ID {request.Id} not found.");

        return new GetInvoiceResponse
        {
            Data = InvoiceMappers.ToDto(invoice),
        };
    }
}
