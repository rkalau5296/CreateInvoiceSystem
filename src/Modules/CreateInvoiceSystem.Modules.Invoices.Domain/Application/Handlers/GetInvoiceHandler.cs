using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;

public class GetInvoiceHandler(IQueryExecutor queryExecutor, IInvoiceRepository _invoiceRepository) : IRequestHandler<GetInvoiceRequest, GetInvoiceResponse>
{
    public async Task<GetInvoiceResponse> Handle(GetInvoiceRequest request, CancellationToken cancellationToken)
    {
        GetInvoiceQuery query = new(request.UserId,request.Id);
        var invoice = await queryExecutor.Execute(query, _invoiceRepository, cancellationToken);

        return new GetInvoiceResponse
        {
            Data = InvoiceMappers.ToDto(invoice),
        };
    }
}
