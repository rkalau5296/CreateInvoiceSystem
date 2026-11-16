using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Security; 

namespace CreateInvoiceSystem.Mail;

public class SmtpEmailService(IConfiguration _configuration) : IEmailService
{
    public async Task SendResetPasswordEmailAsync(string email, string resetLink)
    {
        var message = new MimeMessage();        
        message.From.Add(new MailboxAddress("System Faktur", GetRequiredSmtpSetting("Username")));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = "Resetowanie hasła - System Faktur";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; line-height: 1.6;'>
                <h2>Resetowanie hasła</h2>
                <p>Otrzymaliśmy prośbę o zresetowanie hasła do Twojego konta w Systemie Faktur.</p>
                <p>Aby ustawić nowe hasło, kliknij w poniższy przycisk:</p>
                <p style='margin: 20px 0;'>
                    <a href='{resetLink}' 
                       style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                       Resetuj moje hasło
                    </a>
                </p>
                <p>Link jest ważny przez <strong>2 godziny</strong>.</p>
                <p>Jeśli przycisk nie działa, skopiuj poniższy link do przeglądarki:</p>
                <p style='color: #007bff;'>{resetLink}</p>
                <p>Jeśli to nie Ty prosiłeś o zmianę hasła, po prostu zignoruj tę wiadomość.</p>
                <hr style='border: 0; border-top: 1px solid #eee; margin-top: 20px;'>
                <small style='color: #888;'>Wiadomość została wygenerowana automatycznie, prosimy na nią nie odpowiadać.</small>
            </div>"
        };
        
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

        await client.ConnectAsync(
            GetRequiredSmtpSetting("Host"),
            GetSmtpPort(),
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            GetRequiredSmtpSetting("Username"),
            GetRequiredSmtpSetting("Password"));

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();        
        message.From.Add(new MailboxAddress("System Faktur", GetRequiredSmtpSetting("Username")));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();

        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

        await client.ConnectAsync(
            GetRequiredSmtpSetting("Host"),
            GetSmtpPort(),
            SecureSocketOptions.StartTls,
            cancellationToken);
        
        await client.AuthenticateAsync(
            GetRequiredSmtpSetting("Username"),
            GetRequiredSmtpSetting("Password"),
            cancellationToken);
        
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
    public async Task SendActivationEmailAsync(string email, string activationLink)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("System Faktur", GetRequiredSmtpSetting("Username")));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = "Aktywacja konta - System Faktur";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6;'>
                    <h2>Witaj w Systemie Faktur!</h2>
                    <p>Dziękujemy za rejestrację. Aby korzystać z konta, musisz je najpierw aktywować.</p>
                    <p style='margin: 20px 0;'>
                        <a href='{activationLink}' 
                           style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                           Aktywuj moje konto
                        </a>
                    </p>
                    <p>Jeśli przycisk nie działa, skopiuj poniższy link do przeglądarki:</p>
                    <p style='color: #007bff;'>{activationLink}</p>                    
                    <hr style='border: 0; border-top: 1px solid #eee; margin-top: 20px;'>
                    <small style='color: #888;'>Wiadomość została wygenerowana automatycznie, prosimy na nią nie odpowiadać.</small>
                </div>"
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

        await client.ConnectAsync(
            GetRequiredSmtpSetting("Host"),
            GetSmtpPort(),
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            GetRequiredSmtpSetting("Username"),
            GetRequiredSmtpSetting("Password"));

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
    public async Task SendEmailWithAttachmentAsync(
        string toEmail,
        string subject,
        string htmlBody,
        byte[] attachmentBytes,
        string attachmentFileName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentNullException(nameof(toEmail));
        if (attachmentBytes is null || attachmentBytes.Length == 0)
            throw new ArgumentException("Brak załącznika PDF.", nameof(attachmentBytes));
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("System Faktur", GetRequiredSmtpSetting("Username")));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        
        builder.Attachments.Add(attachmentFileName, attachmentBytes, ContentType.Parse("application/pdf"));
        message.Body = builder.ToMessageBody();
        using var client = new SmtpClient();
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        await client.ConnectAsync(
            GetRequiredSmtpSetting("Host"),
            GetSmtpPort(),
            SecureSocketOptions.StartTls,
            cancellationToken);
        await client.AuthenticateAsync(
            GetRequiredSmtpSetting("Username"),
            GetRequiredSmtpSetting("Password"),
            cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private string GetRequiredSmtpSetting(string name)
    {
        var value = _configuration[$"Smtp:{name}"];
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"Nie skonfigurowano Smtp:{name}.");
    }

    private int GetSmtpPort()
    {
        var value = GetRequiredSmtpSetting("Port");
        return int.TryParse(value, out var port) && port is > 0 and <= 65535
            ? port
            : throw new InvalidOperationException("Nieprawidłowa wartość Smtp:Port.");
    }
}