namespace CreateInvoiceSystem.Modules.Invoices.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using MediatR;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetInvoices;
using CreateInvoiceSystem.Modules.Invoices.Application.Queries;
using CreateInvoiceSystem.Modules.Invoices.Entities;
using CreateInvoiceSystem.Modules.Invoices.Mappers;

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