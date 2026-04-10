namespace CreateInvoiceSystem.Mail;

public interface IEmailService
{
    Task SendResetPasswordEmailAsync(string email, string token);
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken);
    Task SendActivationEmailAsync(string email, string activationLink);
    Task SendEmailWithAttachmentAsync(
        string toEmail,
        string subject,
        string htmlBody,
        byte[] attachmentBytes,
        string attachmentFileName,
        CancellationToken cancellationToken);
}