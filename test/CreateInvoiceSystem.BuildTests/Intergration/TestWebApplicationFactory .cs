using CreateInvoiceSystem.Mail;
using CreateInvoiceSystem.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Intergration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString =
        "Server=(localdb)\\MSSQLLocalDB;"
        + $"Database=CreateInvoiceSystem_IntegrationTests_"
        + $"{Guid.NewGuid():N};"
        + "Trusted_Connection=True;"
        + "TrustServerCertificate=True;";

    public Mock<IEmailService> EmailMock { get; } = new();

    public void ResetEmailMock()
    {
        EmailMock.Reset();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] =
                    "TwojBardzoDlugiISuperTajnyKluczDoGenerowaniaTokenow123!",
                ["Jwt:Issuer"] = "CreateInvoiceSystem",
                ["Jwt:Audience"] = "CreateInvoiceSystemUsers",
                ["Jwt:ExpiryMinutes"] = "15",
                ["FrontendUrl"] = "https://localhost:4200"
            });
        });

        builder.ConfigureServices(services =>
        {
            var servicesToRemove = services
                .Where(descriptor =>
                    descriptor.ServiceType ==
                        typeof(DbContextOptions<CreateInvoiceSystemDbContext>)
                    || descriptor.ServiceType ==
                        typeof(DbContextOptions)
                    || descriptor.ServiceType ==
                        typeof(CreateInvoiceSystemDbContext)
                    || (descriptor.ServiceType.FullName?
                        .StartsWith("Microsoft.EntityFrameworkCore")
                        ?? false))
                .ToList();

            foreach (var descriptor in servicesToRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<CreateInvoiceSystemDbContext>(
                options =>
                    options.UseSqlServer(_connectionString));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                "TestScheme",
                _ => { });

            EmailMock
                .Setup(emailService =>
                    emailService.SendEmailAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            EmailMock
                .Setup(emailService =>
                    emailService.SendEmailWithAttachmentAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<byte[]>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var emailDescriptor = services
                .SingleOrDefault(
                    descriptor =>
                        descriptor.ServiceType == typeof(IEmailService));

            if (emailDescriptor is not null)
            {
                services.Remove(emailDescriptor);
            }

            services.AddSingleton(EmailMock.Object);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        return host;
    }
}