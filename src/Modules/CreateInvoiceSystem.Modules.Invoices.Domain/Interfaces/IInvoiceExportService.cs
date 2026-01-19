using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;

public interface IInvoiceExportService
{
    byte[] ExportToPdf(InvoiceDto invoice);
}
