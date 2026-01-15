using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Security; 

namespace CreateInvoiceSystem.Mail;

public class SmtpEmailService(IConfiguration configuration) : IEmailService
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

        await client.ConnectAsync(
            configuration["Smtp:Host"],
            int.Parse(configuration["Smtp:Port"]!),
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            configuration["Smtp:Username"],
            configuration["Smtp:Password"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("System Faktur", "twoj-email@domena.pl"));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        
        await client.ConnectAsync("smtp.twoj-serwer.pl", 587, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync("login", "haslo");

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
