using CreateInvoiceSystem.Mail;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.API.Adapters.UserEmailAdapter;

public class UserEmailAdapter(IEmailService emailService, IConfiguration _configuration) : IUserEmailSender
{
    public async Task SendResetPasswordEmailAsync(string email, string token)
    {
        var escapedToken = Uri.EscapeDataString(token);
        
        var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/');

        var resetLink = $"{frontendUrl}/reset-password?token={escapedToken}&email={email}";

        await emailService.SendResetPasswordEmailAsync(email, resetLink);
    }
}
