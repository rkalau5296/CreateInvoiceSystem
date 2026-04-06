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

    public async Task SendResetPasswordEmailAsync(string email, string token, string version)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentNullException(nameof(token));

        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentNullException(nameof(version));

        var escapedEmail = Uri.EscapeDataString(email);
        var escapedToken = Uri.EscapeDataString(token);
        var escapedVersion = Uri.EscapeDataString(version);

        var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/');

        if (!Uri.TryCreate(frontendUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException(
                $"BŁĄD KONFIGURACJI: 'FrontendUrl' jest nieprawidłowy lub nieobecny (Wartość: '{frontendUrl}'). " +
                "Sprawdź plik appsettings.json.");
        }

        var resetLink = $"{frontendUrl}/reset-password?token={escapedToken}&email={escapedEmail}&version={escapedVersion}";

        await _emailService.SendResetPasswordEmailAsync(email, resetLink);
    }

    public async Task SendCleanupWarningEmailAsync(string email, string name, int daysLeft, string activationLink)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrWhiteSpace(activationLink))
            throw new ArgumentNullException(nameof(activationLink));

        var subject = "Twoje konto wkrótce wygaśnie";

        var message = $@"
        Witaj {System.Net.WebUtility.HtmlEncode(name)},<br/><br/>
        Zauważyliśmy, że Twoje konto nie zostało jeszcze aktywowane. Zgodnie z naszą polityką bezpieczeństwa, jeśli nie aktywujesz konta w ciągu <b>{daysLeft} dni</b>, Twoje dane zostaną automatycznie usunięte.<br/><br/>
        Aby ułatwić aktywację, wygenerowaliśmy nowy, jednorazowy link aktywacyjny:<br/>
        <p style='margin:20px 0;'>
          <a href='{activationLink}' style='background-color:#007bff;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>Aktywuj konto</a>
        </p>
        <p>Jeśli przycisk nie działa, skopiuj i wklej poniższy link do przeglądarki:</p>
        <p style='color:#007bff;'>{System.Net.WebUtility.HtmlEncode(activationLink)}</p>
        <br/>
        Pozdrawiamy,<br/>Zespół
    ";

        await _emailService.SendEmailAsync(email, subject, message, CancellationToken.None);
    }

}
