using CreateInvoiceSystem.Mail;
using Microsoft.Extensions.Configuration;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class SmtpEmailServiceTests
{
    [Fact]
    public async Task Missing_username_reports_configuration_error()
    {
        var service = new SmtpEmailService(new ConfigurationBuilder().Build());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SendEmailAsync("recipient@example.com", "Subject", "Body", CancellationToken.None));

        Assert.Contains("Smtp:Username", exception.Message);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("65536")]
    [InlineData("invalid")]
    public async Task Invalid_port_reports_configuration_error(string port)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Smtp:Username"] = "sender@example.com",
            ["Smtp:Host"] = "localhost",
            ["Smtp:Port"] = port
        };
        var service = new SmtpEmailService(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SendEmailAsync("recipient@example.com", "Subject", "Body", CancellationToken.None));

        Assert.Contains("Smtp:Port", exception.Message);
    }
}
