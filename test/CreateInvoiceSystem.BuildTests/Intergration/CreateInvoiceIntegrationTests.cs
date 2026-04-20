using CreateInvoiceSystem.Mail;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Intergration;

public class CreateInvoiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CreateInvoiceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {                
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
                                
                var emailMock = new Mock<IEmailService>();
                emailMock.Setup(x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>())) 
                    .Returns(Task.CompletedTask);

                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
                if (descriptor != null) services.Remove(descriptor);
                services.AddSingleton(emailMock.Object);
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Should_CreateInvoice_And_SendPdfEmail()
    {
        int actualUserId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "sprzedawca@test.local");
            if (user == null)
            {
                var address = new AddressEntity
                {
                    Street = "Testowa",
                    Number = "1",
                    City = "Warszawa",
                    PostalCode = "00-100",
                    Country = "Polska"
                };
                db.Set<AddressEntity>().Add(address);
                await db.SaveChangesAsync();

                user = new UserEntity
                {
                    Email = "sprzedawca@test.local",
                    Name = "Sprzedawca",
                    CompanyName = "Testowa Firma Sprzedawcy",
                    Nip = "1234567890",
                    AddressId = address.AddressId
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
            actualUserId = user.Id;
        }

        var invoiceData = new
        {
            Title = "Faktura testowa API",
            Comments = "Brak uwag",
            MethodOfPayment = "Przelew",
            TotalNet = 1000m,
            TotalVat = 230m,
            TotalGross = 1230m,
            PaymentDate = DateTime.UtcNow.AddDays(7),
            CreatedDate = DateTime.UtcNow,
            UserId = actualUserId,
            UserEmail = "sprzedawca@test.local",
            ClientId = (int?)null,
            Client = new
            {
                Name = "Firma Klienta",
                Nip = "0987654321",
                Address = new
                {
                    Street = "Testowa",
                    Number = "1",
                    City = "Warszawa",
                    PostalCode = "00-100",
                    Country = "Polska"
                },
                UserId = actualUserId,
                Email = "klient@test.local"
            },
            SellerName = "Moja Firma",
            SellerNip = "1234567890",
            SellerAddress = "Testowa 1, Warszawa",
            BankAccountNumber = "1234567890",
            ClientName = "Firma Klienta",
            ClientAddress = "Ulica 2, Warszawa",
            ClientNip = "0987654321",
            ClientEmail = "klient@test.local",
            InvoicePositions = new[]
            {
                new
                {
                    InvoicePositionId = 0,
                    InvoiceId = 0,
                    ProductId = (int?)null,
                    Product = new
                    {
                        ProductId = 0,
                        Name = "Produkt",
                        Description = "Opis",
                        Value = 1000m,
                        UserId = actualUserId,
                        IsDeleted = false
                    },
                    ProductName = "Usługa Testowa",
                    ProductDescription = "Opis usługi",
                    ProductValue = 1000m,
                    Quantity = 1,
                    VatRate = "23%"
                }
            }
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
        var json = JsonSerializer.Serialize(invoiceData, options);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Invoice/create", content);
        var resultBody = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: resultBody);
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim("nameid", "1")
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}