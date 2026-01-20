using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
public class GetInvoiceQuery(int? userId, int id) : QueryBase<Invoice, IInvoiceRepository>
{    
    public override async Task<Invoice> Execute(IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken = default)
    {
        return await _invoiceRepository.GetInvoiceByIdAsync(userId, id, cancellationToken)
               ?? throw new InvalidOperationException($"Invoice with ID {id} not found.");
    }
}