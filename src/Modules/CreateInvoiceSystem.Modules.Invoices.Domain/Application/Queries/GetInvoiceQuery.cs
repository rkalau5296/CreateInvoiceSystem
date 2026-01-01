using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
public class GetInvoiceQuery : QueryBase<Invoice, IInvoiceRepository>
{
    private readonly int id;

    public GetInvoiceQuery(int id)
    {
        this.id = id;
    }
    public override async Task<Invoice> Execute(IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken = default)
    {
        return await _invoiceRepository.GetInvoiceByIdAsync(id, cancellationToken)
               ?? throw new InvalidOperationException($"Invoice with ID {id} not found.");
    }
}