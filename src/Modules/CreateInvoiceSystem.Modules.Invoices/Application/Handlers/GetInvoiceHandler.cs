namespace CreateInvoiceSystem.Modules.Invoices.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Invoices.Application.Queries;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetInvoice;
using CreateInvoiceSystem.Modules.Invoices.Mappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class GetInvoiceHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetInvoiceRequest, GetInvoiceResponse>
{
    public async Task<GetInvoiceResponse> Handle(GetInvoiceRequest request, CancellationToken cancellationToken)
    {
        GetInvoiceQuery query = new(request.Id);
        var invoice = await queryExecutor.Execute(query);      

        return new GetInvoiceResponse
        {
            Data = InvoiceMappers.ToDto(invoice),
        };
    }
}
