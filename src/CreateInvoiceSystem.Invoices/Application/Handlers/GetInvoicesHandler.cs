namespace CreateInvoiceSystem.Invoices.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Invoices.Application.Queries;
using CreateInvoiceSystem.Invoices.Application.RequestsResponses.GetInvoices;
using MediatR;
using CreateInvoiceSystem.Abstractions.Mappers;

public class GetInvoicesHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetInvoicesRequest, GetInvoicesResponse>
{
    public async Task<GetInvoicesResponse> Handle(GetInvoicesRequest request, CancellationToken cancellationToken)
    {
        GetInvoicesQuery query = new();

        List<Invoice> invoice = await queryExecutor.Execute(query);

        return new GetInvoicesResponse
        {
            Data = InvoiceMappers.ToDtoList(invoice)
        }; 
    }
}