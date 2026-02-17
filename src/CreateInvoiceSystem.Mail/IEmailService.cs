namespace CreateInvoiceSystem.Mail;

public interface IEmailService
{
    Task SendResetPasswordEmailAsync(string email, string token);
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken);
    Task SendConfirmationRegistrationEmailAsync(string email, string subject, string message);
}
