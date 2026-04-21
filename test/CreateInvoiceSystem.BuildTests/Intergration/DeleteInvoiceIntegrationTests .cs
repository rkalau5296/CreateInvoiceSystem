using CreateInvoiceSystem.BuildTests.Infrastructure;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace CreateInvoiceSystem.BuildTests.Intergration;

public class DeleteInvoiceIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public DeleteInvoiceIntegrationTests(TestWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _factory.ResetEmailMock();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Should_DeleteInvoice_When_RequestIsValid()
    {
        var userId = await SeedUserAsync();
        var invoiceId = await SeedInvoiceAsync(userId);

        var response = await _client.DeleteAsync($"/api/Invoice/{invoiceId}");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var deleted = await db.Set<InvoiceEntity>().FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Should_DeleteInvoicePositions_When_InvoiceIsDeleted()
    {
        var userId = await SeedUserAsync();
        var invoiceId = await SeedInvoiceAsync(userId);

        var response = await _client.DeleteAsync($"/api/Invoice/{invoiceId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var positions = await db.Set<InvoicePositionEntity>()
            .Where(p => p.InvoiceId == invoiceId)
            .ToListAsync();

        positions.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Return404_When_InvoiceDoesNotExist()
    {
        await SeedUserAsync();

        var response = await _client.DeleteAsync("/api/Invoice/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_NotAffectOtherInvoices_When_OneIsDeleted()
    {
        var userId = await SeedUserAsync();
        var invoiceId1 = await SeedInvoiceAsync(userId);
        var invoiceId2 = await SeedInvoiceAsync(userId);

        var response = await _client.DeleteAsync($"/api/Invoice/{invoiceId1}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var remaining = await db.Set<InvoiceEntity>().FirstOrDefaultAsync(i => i.InvoiceId == invoiceId2);
        remaining.Should().NotBeNull();
    }

    private async Task<int> SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var existing = await db.Users.FindAsync(1);
        if (existing != null) return existing.Id;

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
            Id = 1,
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
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var invoice = new InvoiceEntity
        {
            Title = "Faktura do usunięcia",
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
}