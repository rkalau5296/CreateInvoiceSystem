using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;

public interface IInvoiceEmailSender
{
    Task SendInvoiceCreatedEmailAsync(string email, string invoiceNumber, CancellationToken cancellationToken);
    Task SendInvoiceToClientCreatedAsync(InvoiceDto invoiceDto, CancellationToken cancellationToken);
}
