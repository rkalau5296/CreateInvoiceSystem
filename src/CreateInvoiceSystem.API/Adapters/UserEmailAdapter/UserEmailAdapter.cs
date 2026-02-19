using CreateInvoiceSystem.Mail;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.API.Adapters.UserEmailAdapter;

public class UserEmailAdapter(IEmailService _emailService, IConfiguration _configuration) : IUserEmailSender
{
    public async Task SendActivationEmailAsync(string email, string activationLink)
    {
        await _emailService.SendActivationEmailAsync(email, activationLink);
    }

    public async Task SendConfirmationRegistrationEmailAsync(string email, string subject, string message)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        subject ??= "Witamy w serwisie";
        message ??= "Witamy w serwisie!";

        var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/');
        var appLinkHtml = string.IsNullOrWhiteSpace(frontendUrl)
            ? string.Empty
            : $"<p><a href=\"{frontendUrl}\">Przejdź do aplikacji</a></p>";

        var body = $@"
        <!doctype html>
        <html>
          <head><meta charset=""utf-8"" /></head>
          <body style=""font-family:Arial,Helvetica,sans-serif;color:#333;"">
            <h3>{System.Net.WebUtility.HtmlEncode(subject)}</h3>
            <div>{System.Net.WebUtility.HtmlEncode(message)}</div>
            {appLinkHtml}
            <p>Pozdrawiamy,<br/>Zespół</p>
          </body>
        </html>";

        await _emailService.SendEmailAsync(email, subject, body, CancellationToken.None);
    }

    public async Task SendResetPasswordEmailAsync(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        var escapedToken = Uri.EscapeDataString(token);
        
        var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/');

        var resetLink = $"{frontendUrl}/reset-password?token={escapedToken}&email={email}";

        await _emailService.SendResetPasswordEmailAsync(email, resetLink);
    }
    public async Task SendCleanupWarningEmailAsync(string email, string name, int daysLeft)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        var subject = "Twoje konto wkrótce wygaśnie";
        var message = $"Witaj {name},<br/><br/>" +
                      $"Zauważyliśmy, że Twoje konto nie zostało jeszcze aktywowane. " +
                      $"Zgodnie z naszą polityką bezpieczeństwa, jeśli nie aktywujesz konta w ciągu <b>{daysLeft} dni</b>, " +
                      "Twoje dane zostaną automatycznie usunięte.<br/><br/>" +
                      "Jeśli chcesz zachować konto, odszukaj wiadomość z linkiem aktywacyjnym i kliknij go.";
        
        await _emailService.SendEmailAsync(email, subject, message, CancellationToken.None);
    }

}
