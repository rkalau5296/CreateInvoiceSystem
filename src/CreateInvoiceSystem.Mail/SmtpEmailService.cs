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
}