namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoices;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;
using MediatR;

public class GetInvoicesHandler(IQueryExecutor queryExecutor, IInvoiceRepository _invoiceRepository) : IRequestHandler<GetInvoicesRequest, GetInvoicesResponse>
{
    public async Task<GetInvoicesResponse> Handle(GetInvoicesRequest request, CancellationToken cancellationToken)
    {
        GetInvoicesQuery query = new(request.UserId);

        List<Invoice> invoice = await queryExecutor.Execute(query, _invoiceRepository, cancellationToken);

        return new GetInvoicesResponse
        {
            Data = invoice.ToDtoList()
        }; 
    }
}