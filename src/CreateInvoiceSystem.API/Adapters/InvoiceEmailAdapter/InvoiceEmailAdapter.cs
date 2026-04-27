using CreateInvoiceSystem.Mail;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;

namespace CreateInvoiceSystem.API.Adapters.InvoiceEmailAdapter;

public class InvoiceEmailAdapter(
    IEmailService _emailService,
    IInvoiceExportService _pdfExporter) : IInvoiceEmailSender
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

        await _emailService.SendEmailAsync(email, subject, body, cancellationToken);
    }
    public async Task SendInvoiceToClientCreatedAsync(InvoiceDto invoiceDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(invoiceDto.ClientEmail))
            return;
        
        var pdfBytes = await _pdfExporter.ExportToPdfAsync(
            invoiceDto,
            invoiceDto.UserId,
            cancellationToken);
        
        string subject = $"Faktura {invoiceDto.Title}";
        string body = $"""
            <html><body>
            <h2>Szanowny Kliencie,</h2>
            <p>W załączeniu przesyłamy fakturę <strong>{invoiceDto.Title}</strong>.</p>
            <p>Dziękujemy za współpracę.</p>
            <p>Pozdrawiamy,<br/>System Faktur</p>
            </body></html>
            """;
        await _emailService.SendEmailWithAttachmentAsync(
            invoiceDto.ClientEmail,
            subject,
            body,
            pdfBytes,
            $"{invoiceDto.Title.Replace("/", "-")}.pdf",
            cancellationToken);
    }
}
