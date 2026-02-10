using CreateInvoiceSystem.Mail;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.API.Adapters.UserEmailAdapter;

public class UserEmailAdapter(IEmailService emailService) : IUserEmailSender
{
    public async Task SendResetPasswordEmailAsync(string email, string token)
    {      
        var escapedToken = Uri.EscapeDataString(token);

        var resetLink = $"https://localhost:7022/reset-password?token={escapedToken}&email={email}";

        await emailService.SendResetPasswordEmailAsync(email, resetLink);
    }
}
