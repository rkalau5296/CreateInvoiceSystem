namespace CreateInvoiceSystem.Mail;

public interface IEmailService
{
    Task<bool> SendEmailAsync(Email email);
}
