using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
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

public class UpdateInvoiceIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public UpdateInvoiceIntegrationTests(TestWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _factory.ResetEmailMock();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Should_UpdateInvoice_When_RequestIsValid()
    {
        var userId = await SeedUserAsync();
        var invoiceId = await SeedInvoiceAsync(userId);

        var updatePayload = BuildUpdatePayload(invoiceId, userId, methodOfPayment: "Gotówka", totalGross: 2460m);

        var response = await _client.PutAsJsonAsync($"/api/Invoice/update/{invoiceId}", updatePayload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var updated = await db.Set<InvoiceEntity>().FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
        updated.Should().NotBeNull();
        updated!.MethodOfPayment.Should().Be("Gotówka");
        updated.TotalGross.Should().Be(2460m);
    }

    [Fact]
    public async Task Should_UpdateInvoicePositions_When_RequestIsValid()
    {
        var userId = await SeedUserAsync();
        var invoiceId = await SeedInvoiceAsync(userId);

        var updatePayload = BuildUpdatePayload(invoiceId, userId, productName: "Nowy Produkt", productValue: 2000m, quantity: 2);

        var response = await _client.PutAsJsonAsync($"/api/Invoice/update/{invoiceId}", updatePayload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

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

        var updatePayload = BuildUpdatePayload(invoiceId, userId, methodOfPayment: "");

        var response = await _client.PutAsJsonAsync($"/api/Invoice/update/{invoiceId}", updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return404_When_InvoiceDoesNotExist()
    {
        var userId = await SeedUserAsync();
        var updatePayload = BuildUpdatePayload(99999, userId);

        var response = await _client.PutAsJsonAsync("/api/Invoice/update/99999", updatePayload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
        decimal totalGross = 1230m,
        string productName = "Usługa Testowa",
        decimal productValue = 1000m,
        int quantity = 1)
    {
        return new
        {
            InvoiceId = invoiceId,
            Title = "Faktura zaktualizowana",
            TotalNet = 1000m,
            TotalVat = 230m,
            TotalGross = totalGross,
            PaymentDate = DateTime.UtcNow.AddDays(14),
            CreatedDate = DateTime.UtcNow,
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
                    VatRate = "23%"
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