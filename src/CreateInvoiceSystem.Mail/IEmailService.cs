namespace CreateInvoiceSystem.Mail;

public interface IEmailService
{
    Task SendResetPasswordEmailAsync(string email, string token);
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken);
    
}
