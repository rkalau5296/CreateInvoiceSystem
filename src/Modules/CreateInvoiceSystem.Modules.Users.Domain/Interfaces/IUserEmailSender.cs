namespace CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

public interface IUserEmailSender
{
    Task SendResetPasswordEmailAsync(string email, string token);
    Task SendConfirmationRegistrationEmailAsync(string email, string subject, string message);
}
