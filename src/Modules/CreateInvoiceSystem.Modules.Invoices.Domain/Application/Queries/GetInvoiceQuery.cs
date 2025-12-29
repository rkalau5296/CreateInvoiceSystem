using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
public class GetInvoiceQuery(int id) : QueryBase<Invoice, IInvoiceRepository>
{    
    public int Id { get; } = id;

    public override async Task<Invoice> Execute(IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetInvoiceByIdAsync(
            invoiceId: Id,
            includeClient: true,
            includeClientAddress: true,
            includePositions: true,
            includeProducts: true,
            cancellationToken: cancellationToken);

        return invoice ?? throw new InvalidOperationException($"Invoice with ID {Id} not found.");
    }
}