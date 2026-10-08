using CreateInvoiceSystem.Invoices.Persistence.Shared.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Shared.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace CreateInvoiceSystem.BuildTests.Integration;

[Collection("Integration tests")]
public class DeleteInvoiceIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _integrationTestFixture;
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeleteInvoiceIntegrationTests(
        IntegrationTestFixture integrationTestFixture)
    {
        _integrationTestFixture = integrationTestFixture;
        _factory = integrationTestFixture.Factory;
        _factory.ResetEmailMock();
        _client = _factory.CreateClient();
    }

    public async ValueTask InitializeAsync()
    {
        await _integrationTestFixture.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task Should_DeleteInvoice_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var userId = await SeedUserAsync(cancellationToken);
        var invoiceId = await SeedInvoiceAsync(
            userId,
            cancellationToken: cancellationToken);

        var response = await _client.DeleteAsync(
            $"/api/Invoice/{invoiceId}",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            because: body);

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var deleted = await db.Set<InvoiceEntity>()
            .FirstOrDefaultAsync(
                invoice => invoice.InvoiceId == invoiceId,
                cancellationToken);

        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Should_DeleteInvoicePositions_When_InvoiceIsDeleted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var userId = await SeedUserAsync(cancellationToken);
        var invoiceId = await SeedInvoiceAsync(
            userId,
            cancellationToken: cancellationToken);

        var response = await _client.DeleteAsync(
            $"/api/Invoice/{invoiceId}",
            cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var positions = await db.Set<InvoicePositionEntity>()
            .Where(position => position.InvoiceId == invoiceId)
            .ToListAsync(cancellationToken);

        positions.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Return404_When_InvoiceDoesNotExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await SeedUserAsync(cancellationToken);

        var response = await _client.DeleteAsync(
            "/api/Invoice/99999",
            cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_NotAffectOtherInvoices_When_OneIsDeleted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var userId = await SeedUserAsync(cancellationToken);

        var invoiceId1 = await SeedInvoiceAsync(
            userId,
            "1/09/2026",
            cancellationToken);

        var invoiceId2 = await SeedInvoiceAsync(
            userId,
            "2/09/2026",
            cancellationToken);

        var response = await _client.DeleteAsync(
            $"/api/Invoice/{invoiceId1}",
            cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var remaining = await db.Set<InvoiceEntity>()
            .FirstOrDefaultAsync(
                invoice => invoice.InvoiceId == invoiceId2,
                cancellationToken);

        remaining.Should().NotBeNull();
    }

    private async Task<int> SeedUserAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var existing = await db.Users.FindAsync(
            new object[] { 1 },
            cancellationToken);

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

        await db.SaveChangesAsync(cancellationToken);

        var user = new UserEntity
        {
            Email = "sprzedawca@test.local",
            Name = "Sprzedawca",
            CompanyName = "Testowa Firma Sprzedawcy",
            Nip = "1234567890",
            AddressId = address.AddressId
        };

        db.Users.Add(user);

        await db.SaveChangesAsync(cancellationToken);

        return user.Id;
    }

    private async Task<int> SeedInvoiceAsync(
        int userId,
        string title = "Faktura do edycji",
        CancellationToken cancellationToken = default)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var invoice = new InvoiceEntity
        {
            Title = title,
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

        await db.SaveChangesAsync(cancellationToken);

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

        await db.SaveChangesAsync(cancellationToken);

        return invoice.InvoiceId;
    }
}