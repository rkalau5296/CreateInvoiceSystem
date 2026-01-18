using CreateInvoiceSystem.Mail;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;

namespace CreateInvoiceSystem.API.Adapters.InvoiceEmailAdapter;

public class InvoiceEmailAdapter(IEmailService emailService) : IInvoiceEmailSender
{
    public async Task SendInvoiceCreatedEmailAsync(string email, string invoiceNumber, CancellationToken cancellationToken)
    {
        string subject = $"Potwierdzenie: Faktura {invoiceNumber}";
        string body = $@"
            <html>
                <body>
                    <h2>Dzień dobry!</h2>
                    <p>Faktura o numerze <strong>{invoiceNumber}</strong> została poprawnie wygenerowana w systemie.</p>
                    <p>Pozdrawiamy,<br/>Twój System Faktur</p>
                </body>
            </html>";

        await emailService.SendEmailAsync(email, subject, body, cancellationToken);
    }
}
