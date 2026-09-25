using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoices;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;

public class GetInvoicesHandler(IInvoiceRepository _invoiceRepository) : IRequestHandler<GetInvoicesRequest, GetInvoicesResponse>
{
    public async Task<GetInvoicesResponse> Handle(GetInvoicesRequest request, CancellationToken cancellationToken)
    {
        PagedResult<Invoice> pagedResult = await _invoiceRepository.GetInvoicesAsync(
            request.UserId, request.PageNumber, request.PageSize, request.SearchTerm, cancellationToken)
            ?? throw new InvalidOperationException($"List of invoices is empty."); 

        return new GetInvoicesResponse
        {
            Data = pagedResult.Items.ToDtoList(),
            TotalCount = pagedResult.TotalCount
        };
    }
}