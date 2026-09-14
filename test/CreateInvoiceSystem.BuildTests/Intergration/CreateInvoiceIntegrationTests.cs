using CreateInvoiceSystem.Invoices.Persistence.Shared.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Shared.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit.Abstractions;

namespace CreateInvoiceSystem.BuildTests.Intergration;

[Collection("Integration tests")]
public class CreateInvoiceIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _integrationTestFixture;
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateInvoiceIntegrationTests(IntegrationTestFixture integrationTestFixture)
    {
        _integrationTestFixture = integrationTestFixture;
        _factory = integrationTestFixture.Factory;
        _factory.ResetEmailMock();
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync()
    {
        return _integrationTestFixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Should_CreateInvoice_And_SendPdfEmail()
    {
        var userId = await SeedUserAsync();
        const string clientName = "Firma Klienta";
        var clientNip = CreateTestNip();

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
            UserId = userId,
            UserEmail = "sprzedawca@test.local",
            ClientId = (int?)null,
            Client = new
            {
                Name = clientName,
                Nip = clientNip,
                Email = "klient@test.local",
                Address = new
                {
                    Street = "Testowa",
                    Number = "1",
                    City = "Warszawa",
                    PostalCode = "00-100",
                    Country = "Polska"
                },
                UserId = userId
            },
            SellerName = "Testowa Firma Sprzedawcy",
            SellerNip = "1234567890",
            SellerAddress = "Testowa 1, Warszawa",
            BankAccountNumber = "1234567890",
            ClientName = clientName,
            ClientAddress = "Ulica 2, Warszawa",
            ClientNip = clientNip,
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
                        UserId = userId,
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

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync(
            "/api/Invoice/create",
            content);

        var resultBody = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            because: resultBody);

        using var document = JsonDocument.Parse(resultBody);
        var data = document.RootElement.GetProperty("data");

        data.GetProperty("sellerName")
            .GetString()
            .Should()
            .Be("Testowa Firma Sprzedawcy");

        data.GetProperty("clientName")
            .GetString()
            .Should()
            .Be(clientName);

        data.GetProperty("methodOfPayment")
            .GetString()
            .Should()
            .Be("Przelew");
    }

    [Fact]
    public async Task Should_CreateInvoice_And_SaveToDatabase_When_RequestIsValid()
    {
        var userId = await SeedUserAsync();
        var clientNip = CreateTestNip();
        var clientName = $"Firma Klienta {Guid.NewGuid():N}";
        var clientEmail = $"klient_{Guid.NewGuid():N}@test.local";

        var invoice = BuildInvoicePayload(
            userId,
            clientEmail: clientEmail,
            clientNip: clientNip,
            clientName: clientName);

        var response = await _client.PostAsJsonAsync(
            "/api/Invoice/create",
            invoice);

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            because: body);

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var saved = await db
            .Set<InvoiceEntity>()
            .SingleOrDefaultAsync(entity =>
                entity.UserId == userId
                && entity.ClientNip == clientNip
                && entity.ClientEmail == clientEmail);

        saved.Should().NotBeNull();

        saved!.TotalGross.Should().Be(1230m);
        saved.ClientName.Should().Be(clientName);
        saved.ClientNip.Should().Be(clientNip);
        saved.ClientEmail.Should().Be(clientEmail);
        saved.MethodOfPayment.Should().Be("Przelew");

        var positions = await db
            .Set<InvoicePositionEntity>()
            .Where(position =>
                position.InvoiceId == saved.InvoiceId)
            .ToListAsync();

        positions.Should().ContainSingle();

        var position = positions.Single();

        position.ProductName.Should().Be("Usługa Testowa");
        position.ProductValue.Should().Be(1000m);
        position.Quantity.Should().Be(1);
        position.VatRate.Should().Be("23%");
    }

    [Fact]
    public async Task Should_SendPdfToClient_When_ClientEmailIsProvided()
    {
        var userId = await SeedUserAsync();

        var invoice = BuildInvoicePayload(
            userId,
            clientEmail: "klient@test.local");

        var response = await _client.PostAsJsonAsync(
            "/api/Invoice/create",
            invoice);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _factory.EmailMock.Verify(
            emailService => emailService.SendEmailWithAttachmentAsync(
                "klient@test.local",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Should_NotSendPdfToClient_When_ClientEmailIsNotProvided()
    {
        var userId = await SeedUserAsync();

        var invoice = BuildInvoicePayload(
            userId,
            clientEmail: null);

        var response = await _client.PostAsJsonAsync(
            "/api/Invoice/create",
            invoice);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _factory.EmailMock.Verify(
            emailService => emailService.SendEmailWithAttachmentAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_AlwaysSendConfirmationEmailToSeller()
    {
        var userId = await SeedUserAsync();

        var invoice = BuildInvoicePayload(
            userId,
            clientEmail: null);

        var response = await _client.PostAsJsonAsync(
            "/api/Invoice/create",
            invoice);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _factory.EmailMock.Verify(
            emailService => emailService.SendEmailAsync(
                "sprzedawca@test.local",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Should_Return400_When_TitleIsMissing()
    {
        var userId = await SeedUserAsync();

        var invoice = BuildInvoicePayload(
            userId,
            clientEmail: "klient@test.local");

        var payload = (dynamic)invoice;

        var brokenPayload = new
        {
            Title = string.Empty,
            payload.Comments,
            payload.MethodOfPayment,
            payload.TotalNet,
            payload.TotalVat,
            payload.TotalGross,
            payload.PaymentDate,
            payload.CreatedDate,
            payload.UserId,
            payload.UserEmail,
            payload.ClientId,
            payload.Client,
            payload.SellerName,
            payload.SellerNip,
            payload.SellerAddress,
            payload.BankAccountNumber,
            payload.ClientName,
            payload.ClientAddress,
            payload.ClientNip,
            payload.ClientEmail,
            payload.InvoicePositions
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Invoice/create",
            brokenPayload);

        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);
    }

    private async Task<int> SeedUserAsync()
    {
        const string email = "sprzedawca@test.local";

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var existing = await db.Users
            .SingleOrDefaultAsync(user => user.Email == email);

        if (existing is not null)
        {
            return existing.Id;
        }

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

        var user = new UserEntity
        {
            Email = email,
            UserName = email,
            Name = "Sprzedawca",
            CompanyName = "Testowa Firma Sprzedawcy",
            Nip = CreateTestNip(),
            AddressId = address.AddressId,
            EmailConfirmed = true,
            IsActive = true
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user.Id;
    }

    private static object BuildInvoicePayload(
        int userId,
        string? clientEmail,
        string? clientNip = null,
        string clientName = "Firma Klienta")
    {
        clientNip ??= CreateTestNip();

        return new
        {
            Title = "Faktura testowa API",
            Comments = "Brak uwag",
            MethodOfPayment = "Przelew",
            TotalNet = 1000m,
            TotalVat = 230m,
            TotalGross = 1230m,
            PaymentDate = DateTime.UtcNow.AddDays(7),
            CreatedDate = DateTime.UtcNow,
            UserId = userId,
            UserEmail = "sprzedawca@test.local",
            ClientId = (int?)null,

            Client = new
            {
                Name = clientName,
                Nip = clientNip,
                Address = new
                {
                    Street = "Testowa",
                    Number = "1",
                    City = "Warszawa",
                    PostalCode = "00-100",
                    Country = "Polska"
                },
                UserId = userId,
                Email = clientEmail ?? string.Empty
            },

            SellerName = "Moja Firma",
            SellerNip = "1234567890",
            SellerAddress = "Testowa 1, Warszawa",
            BankAccountNumber = "1234567890",

            ClientName = clientName,
            ClientAddress = "Ulica 2, Warszawa",
            ClientNip = clientNip,
            ClientEmail = clientEmail,

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
                        UserId = userId,
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
    }

    private static string CreateTestNip()
    {
        return Random.Shared
            .NextInt64(1000000000, 9999999999)
            .ToString();
    }
}