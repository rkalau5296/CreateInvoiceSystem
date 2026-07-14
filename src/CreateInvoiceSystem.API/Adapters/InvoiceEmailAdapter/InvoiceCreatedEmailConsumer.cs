using CreateInvoiceSystem.Mail;
using CreateInvoiceSystem.Messaging.Contracts;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using MassTransit;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;


namespace CreateInvoiceSystem.API.Adapters.InvoiceEmailAdapter
{
    public class InvoiceCreatedEmailConsumer(
    IEmailService _emailService,
    IInvoiceExportService _pdfExporter,
    IInvoiceRepository _invoiceRepository) : IConsumer<InvoiceCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<InvoiceCreatedIntegrationEvent> context)
        {
            var msg = context.Message;
            var ct = context.CancellationToken;

            var invoiceEntity = await _invoiceRepository.GetInvoiceByIdAsync(msg.UserId, msg.InvoiceId, ct);
            if (invoiceEntity is null)
                return;

            var invoice = invoiceEntity.ToDto();
                        
            var userEmail = await _invoiceRepository.GetUserEmailByIdAsync(msg.UserId, ct);
            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                string userSubject = $"Potwierdzenie: Faktura {invoice.Title}";
                string userBody = $@"
                <html><body>
                    <h2>Dzień dobry!</h2>
                    <p>Faktura o numerze <strong>{invoice.Title}</strong> została poprawnie wygenerowana w systemie.</p>
                    <p>Pozdrawiamy,<br/>Twój System Faktur</p>
                </body></html>";

                await _emailService.SendEmailAsync(userEmail, userSubject, userBody, ct);
            }
            
            if (string.IsNullOrWhiteSpace(invoice.ClientEmail))
                return;

            var pdfBytes = await _pdfExporter.ExportToPdfAsync(invoice, invoice.UserId, ct);

            string clientSubject = $"Faktura {invoice.Title}";
            string clientBody = $"""
            <html><body>
            <h2>Szanowny Kliencie,</h2>
            <p>W załączeniu przesyłamy fakturę <strong>{invoice.Title}</strong>.</p>
            <p>Dziękujemy za współpracę.</p>
            <p>Pozdrawiamy,<br/>System Faktur</p>
            </body></html>
            """;

            await _emailService.SendEmailWithAttachmentAsync(
                invoice.ClientEmail, clientSubject, clientBody, pdfBytes,
                $"{invoice.Title.Replace("/", "-")}.pdf", ct);
        }
    }
}
