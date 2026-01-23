using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
public class GetInvoicesQuery(int? userId, int pageNumber, int pageSize, string? searchTerm) : QueryBase<PagedResult<Invoice>, IInvoiceRepository>
{
    public override async Task<PagedResult<Invoice>> Execute(IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken = default)
    {        
        return await _invoiceRepository.GetInvoicesAsync(userId, pageNumber, pageSize, searchTerm, cancellationToken)
            ?? throw new InvalidOperationException($"List of invoices is empty.");
    }
}
