using CreateInvoiceSystem.Modules.Invoices.Dto;

namespace CreateInvoiceSystem.Modules.Invoices.Interfaces;

public interface IInvoiceEmailSender
{
    Task SendInvoiceCreatedEmailAsync(string email, string invoiceNumber, CancellationToken cancellationToken);
    Task SendInvoiceToClientCreatedAsync(InvoiceDto invoiceDto, CancellationToken cancellationToken);
}
