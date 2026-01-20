namespace CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

public interface IUserEmailSender
{
    Task SendResetPasswordEmailAsync(string email, string token);
}
