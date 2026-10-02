using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;

public interface IInvoiceExportService
{
    Task<byte[]> ExportToPdfAsync(InvoiceDto invoice, int userId, CancellationToken cancellationToken);
}
