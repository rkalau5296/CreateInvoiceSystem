using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using CreateInvoiceSystem.API;
using CreateInvoiceSystem.Modules.Invoices.Domain.Controllers;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Invoices.Commands
{
    public class InvoicesApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var moduleAssembly = typeof(InvoiceController).Assembly;
                services.AddControllers()
                    .AddApplicationPart(moduleAssembly)
                    .AddControllersAsServices();

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            });
        }
    }

    public class CreateInvoiceIntegrationTests : IClassFixture<InvoicesApiFactory>
    {
        private readonly HttpClient _client;

        public CreateInvoiceIntegrationTests(InvoicesApiFactory factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        }

        [Fact]
        public async Task Should_CreateInvoice_And_SendPdfEmail()
        {
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
                UserId = 1,
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
                    UserId = 1,
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
                            UserId = 1,
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

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };

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
}