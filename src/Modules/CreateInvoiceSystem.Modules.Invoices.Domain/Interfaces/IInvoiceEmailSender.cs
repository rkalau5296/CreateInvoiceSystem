namespace CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;

public interface IInvoiceEmailSender
{
    Task SendInvoiceCreatedEmailAsync(string email, string invoiceNumber, CancellationToken cancellationToken);
}
