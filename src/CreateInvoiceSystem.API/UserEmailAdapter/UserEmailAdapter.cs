using CreateInvoiceSystem.Mail;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.API.UserEmailAdapter;

public class UserEmailAdapter(IEmailService emailService) : IUserEmailSender
{
    public async Task SendResetPasswordEmailAsync(string email, string token)
    {
        var resetLink = $"https://localhost:7168/api/auth/reset-password?token={token}&email={email}";

        await emailService.SendResetPasswordEmailAsync(email, resetLink);
    }
}
