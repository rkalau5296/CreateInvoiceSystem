using CreateInvoiceSystem.Invoices.Persistence.Shared.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Shared.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace CreateInvoiceSystem.BuildTests.Intergration;

[Collection("Integration tests")]
public class UpdateInvoiceIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _integrationTestFixture;
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UpdateInvoiceIntegrationTests(
        IntegrationTestFixture integrationTestFixture,
        ITestOutputHelper output)
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
    public async Task Should_UpdateInvoice_When_RequestIsValid()
    {
        var userId = await SeedUserAsync();
        var invoiceId = await SeedInvoiceAsync(userId);

        var updatePayload = BuildUpdatePayload(
            invoiceId,
            userId,
            methodOfPayment: "Gotówka",
            totalGross: 2460m);

        var response = await _client.PutAsJsonAsync(
            $"/api/Invoice/update/{invoiceId}",
            updatePayload);

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            because: body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var updated = await db.Set<InvoiceEntity>()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

        updated.Should().NotBeNull();
        updated!.MethodOfPayment.Should().Be("Gotówka");
        updated.TotalGross.Should().Be(2460m);
    }

    [Fact]
    public async Task Should_UpdateInvoicePositions_When_RequestIsValid()
    {
        var userId = await SeedUserAsync();
        var invoiceId = await SeedInvoiceAsync(userId);

        var updatePayload = BuildUpdatePayload(
            invoiceId,
            userId,
            productName: "Nowy Produkt",
            productValue: 2000m,
            quantity: 2);

        var response = await _client.PutAsJsonAsync(
            $"/api/Invoice/update/{invoiceId}",
            updatePayload);

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            because: body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var positions = await db.Set<InvoicePositionEntity>()
            .Where(p => p.InvoiceId == invoiceId)
            .ToListAsync();

        positions.Should().NotBeEmpty();
        positions.Should().Contain(p => p.ProductName == "Nowy Produkt");
        positions.Should().Contain(p => p.ProductValue == 2000m);
        positions.Should().Contain(p => p.Quantity == 2);
    }

    [Fact]
    public async Task Should_Return400_When_MethodOfPaymentIsMissing()
    {
        var userId = await SeedUserAsync();
        var invoiceId = await SeedInvoiceAsync(userId);

        var updatePayload = BuildUpdatePayload(
            invoiceId,
            userId,
            methodOfPayment: string.Empty);

        var response = await _client.PutAsJsonAsync(
            $"/api/Invoice/update/{invoiceId}",
            updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return404_When_InvoiceDoesNotExist()
    {
        var userId = await SeedUserAsync();

        var updatePayload = BuildUpdatePayload(
            99999,
            userId);

        var response = await _client.PutAsJsonAsync(
            "/api/Invoice/update/99999",
            updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // --- NOWE TESTY WALIDACYJNE DLA UPDATE ---

    [Fact]
    public async Task Should_UpdateInvoice_When_TotalVatIsZeroOrNull_And_VatRateIsExempt()
    {
        var userId = await SeedUserAsync();
        var invoiceId = await SeedInvoiceAsync(userId);

        var updatePayload = BuildUpdatePayload(
            invoiceId,
            userId,
            totalNet: 1000m,
            totalVat: 0m,
            totalGross: 1000m,
            vatRate: "zw");

        var response = await _client.PutAsJsonAsync(
            $"/api/Invoice/update/{invoiceId}",
            updatePayload);

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var updated = await db.Set<InvoiceEntity>()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

        updated.Should().NotBeNull();
        updated!.TotalVat.Should().Be(0m);
        updated.TotalGross.Should().Be(1000m);
    }

    [Fact]
    public async Task Should_Return400_When_UpdateTotalNetIsZeroOrNegative()
    {
        var userId = await SeedUserAsync();
        var invoiceId = await SeedInvoiceAsync(userId);

        var updatePayload = BuildUpdatePayload(
            invoiceId,
            userId,
            totalNet: 0m);

        var response = await _client.PutAsJsonAsync(
            $"/api/Invoice/update/{invoiceId}",
            updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_When_UpdateAmountsHaveMoreThanTwoDecimalPlaces()
    {
        var userId = await SeedUserAsync();
        var invoiceId = await SeedInvoiceAsync(userId);

        var updatePayload = BuildUpdatePayload(
            invoiceId,
            userId,
            totalNet: 1000.555m);

        var response = await _client.PutAsJsonAsync(
            $"/api/Invoice/update/{invoiceId}",
            updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_When_UpdateCreatedDateIsInTheFuture()
    {
        var userId = await SeedUserAsync();
        var invoiceId = await SeedInvoiceAsync(userId);

        var updatePayload = BuildUpdatePayload(
            invoiceId,
            userId,
            createdDate: DateTime.UtcNow.AddDays(2));

        var response = await _client.PutAsJsonAsync(
            $"/api/Invoice/update/{invoiceId}",
            updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_When_UpdatePaymentDateIsEarlierThanCreatedDate()
    {
        var userId = await SeedUserAsync();
        var invoiceId = await SeedInvoiceAsync(userId);
        var createdDate = DateTime.UtcNow;

        var updatePayload = BuildUpdatePayload(
            invoiceId,
            userId,
            createdDate: createdDate,
            paymentDate: createdDate.AddDays(-1));

        var response = await _client.PutAsJsonAsync(
            $"/api/Invoice/update/{invoiceId}",
            updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- HELPERY ---

    private async Task<int> SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var existing = await db.Users.FindAsync(1);

        if (existing != null)
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
            Email = "sprzedawca@test.local",
            Name = "Sprzedawca",
            CompanyName = "Testowa Firma Sprzedawcy",
            Nip = "1234567890",
            AddressId = address.AddressId
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user.Id;
    }

    private async Task<int> SeedInvoiceAsync(int userId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var invoice = new InvoiceEntity
        {
            Title = "Faktura do edycji",
            MethodOfPayment = "Przelew",
            TotalNet = 1000m,
            TotalVat = 230m,
            TotalGross = 1230m,
            PaymentDate = DateTime.UtcNow.AddDays(7),
            CreatedDate = DateTime.UtcNow,
            UserId = userId,
            ClientName = "Firma Klienta",
            ClientNip = "0987654321",
            ClientAddress = "Testowa 1, Warszawa",
            ClientEmail = "klient@test.local",
            SellerName = "Testowa Firma Sprzedawcy",
            SellerNip = "1234567890",
            SellerAddress = "Testowa 1, Warszawa",
            BankAccountNumber = "1234567890",
            Comments = "Brak uwag"
        };

        db.Set<InvoiceEntity>().Add(invoice);
        await db.SaveChangesAsync();

        var position = new InvoicePositionEntity
        {
            InvoiceId = invoice.InvoiceId,
            ProductName = "Usługa Testowa",
            ProductDescription = "Opis usługi",
            ProductValue = 1000m,
            Quantity = 1,
            VatRate = "23%"
        };

        db.Set<InvoicePositionEntity>().Add(position);
        await db.SaveChangesAsync();

        return invoice.InvoiceId;
    }

    private static object BuildUpdatePayload(
        int invoiceId,
        int userId,
        string methodOfPayment = "Przelew",
        decimal totalNet = 1000m,
        decimal totalVat = 230m,
        decimal totalGross = 1230m,
        string productName = "Usługa Testowa",
        decimal productValue = 1000m,
        int quantity = 1,
        string vatRate = "23%",
        DateTime? createdDate = null,
        DateTime? paymentDate = null)
    {
        var now = createdDate ?? DateTime.UtcNow;
        var payDate = paymentDate ?? now.AddDays(14);

        return new
        {
            InvoiceId = invoiceId,
            Title = "Faktura zaktualizowana",
            TotalNet = totalNet,
            TotalVat = totalVat,
            TotalGross = totalGross,
            PaymentDate = payDate,
            CreatedDate = now,
            Comments = "Zaktualizowano",
            ClientId = (int?)null,
            UserId = userId,

            Client = new
            {
                Name = "Firma Klienta",
                Nip = "0987654321",
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

            MethodOfPayment = methodOfPayment,

            InvoicePositions = new[]
            {
                new
                {
                    InvoicePositionId = 0,
                    InvoiceId = invoiceId,
                    ProductId = (int?)null,
                    ProductName = productName,
                    ProductDescription = "Opis usługi",
                    ProductValue = productValue,
                    Quantity = quantity,
                    VatRate = vatRate
                }
            },

            SellerName = "Testowa Firma Sprzedawcy",
            SellerNip = "1234567890",
            SellerAddress = "Testowa 1, Warszawa",
            BankAccountNumber = "1234567890",
            ClientName = "Firma Klienta",
            ClientNip = "0987654321",
            ClientAddress = "Testowa 1, Warszawa",
            ClientEmail = "klient@test.local"
        };
    }
}