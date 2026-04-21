using CreateInvoiceSystem.BuildTests.Intergration;
using CreateInvoiceSystem.Mail;
using CreateInvoiceSystem.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IEmailService> EmailMock { get; } = new();
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    public void ResetEmailMock() => EmailMock.Reset();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TwojBardzoDlugiISuperTajnyKluczDoGenerowaniaTokenow123!",
                ["Jwt:Issuer"] = "CreateInvoiceSystem",
                ["Jwt:Audience"] = "CreateInvoiceSystemUsers",
                ["Jwt:ExpiryMinutes"] = "15",
                ["FrontendUrl"] = "https://localhost:4200"
            });
        });

        builder.ConfigureServices(services =>
        {
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<CreateInvoiceSystemDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(CreateInvoiceSystemDbContext) ||
                (d.ServiceType.FullName?.StartsWith("Microsoft.EntityFrameworkCore") ?? false)
            ).ToList();

            foreach (var d in toRemove)
                services.Remove(d);

            services.AddDbContext<CreateInvoiceSystemDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

            EmailMock.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            EmailMock.Setup(x => x.SendEmailWithAttachmentAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var emailDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
            if (emailDescriptor != null) services.Remove(emailDescriptor);
            services.AddSingleton(EmailMock.Object);
        });
    }
}