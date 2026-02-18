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
        message.From.Add(new MailboxAddress("System Faktur", "no-reply@system.pl"));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = "Resetowanie hasła";

        message.Body = new TextPart("plain")
        {
            Text = $"Aby zresetować hasło, kliknij w poniższy link:\n\n{resetLink}\n\n" +
                   $"Link jest ważny przez 2 godziny."
        };

        using var client = new SmtpClient();
        
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

        await client.ConnectAsync(
            _configuration["Smtp:Host"],
            int.Parse(_configuration["Smtp:Port"]!),
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            _configuration["Smtp:Username"],
            _configuration["Smtp:Password"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
    public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();        
        message.From.Add(new MailboxAddress("System Faktur", _configuration["Smtp:Username"]));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();

        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

        await client.ConnectAsync(
            _configuration["Smtp:Host"],
            int.Parse(_configuration["Smtp:Port"]),
            SecureSocketOptions.StartTls,
            cancellationToken);
        
        await client.AuthenticateAsync(
            _configuration["Smtp:Username"],
            _configuration["Smtp:Password"],
            cancellationToken);
        
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
    public async Task SendActivationEmailAsync(string email, string activationLink)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("System Faktur", _configuration["Smtp:Username"]));
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
                    <p>{activationLink}</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin-top: 20px;'>
                    <small style='color: #888;'>Wiadomość została wygenerowana automatycznie, prosimy na nią nie odpowiadać.</small>
                </div>"
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

        await client.ConnectAsync(
            _configuration["Smtp:Host"],
            int.Parse(_configuration["Smtp:Port"]!),
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            _configuration["Smtp:Username"],
            _configuration["Smtp:Password"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}