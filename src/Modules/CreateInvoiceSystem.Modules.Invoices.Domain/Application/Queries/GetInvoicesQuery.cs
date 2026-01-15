using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
public class GetInvoicesQuery(int? userId) : QueryBase<List<Invoice>, IInvoiceRepository>
{
    public override async Task<List<Invoice>> Execute(IInvoiceRepository invoiceRepository, CancellationToken cancellationToken = default)
    {
        return await invoiceRepository.GetInvoicesAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException($"List of addresses is empty.");
    }
}
