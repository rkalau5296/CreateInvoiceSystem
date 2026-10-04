using CreateInvoiceSystem.Modules.Invoices.Dto;

namespace CreateInvoiceSystem.Modules.Invoices.Interfaces;

public interface IInvoiceExportService
{
    Task<byte[]> ExportToPdfAsync(InvoiceDto invoice, int userId, CancellationToken cancellationToken);
}
