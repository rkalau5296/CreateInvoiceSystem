using CreateInvoiceSystem.BuildTests.Infrastructure;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit.Abstractions;

namespace CreateInvoiceSystem.BuildTests.Intergration;

public class ExportControllerIntergrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public ExportControllerIntergrationTests(TestWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _factory.ResetEmailMock();
        _client = _factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/export/invoices", "faktury.csv")]
    [InlineData("/api/export/products", "produkty.csv")]
    [InlineData("/api/export/clients", "klienci.csv")]
    public async Task Should_DownloadCsv_When_UserIsAuthenticated(string url, string expectedFileName)
    {
        await SeedUserAndDataAsync();

        var response = await _client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
        response.Content.Headers.ContentDisposition?.FileName.Should().Be(expectedFileName);

        var content = await response.Content.ReadAsByteArrayAsync();
        content.Length.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("/api/export/invoices")]
    [InlineData("/api/export/products")]
    [InlineData("/api/export/clients")]
    public async Task Should_ReturnEmptyCsv_When_UserHasNoData(string url)
    {
        await SeedUserWithoutDataAsync();

        var response = await _client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsByteArrayAsync();
        content.Length.Should().BeGreaterThan(0);
    }

    private async Task SeedUserAndDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var existingUser = await db.Users.FindAsync(1);
        if (existingUser != null) return;

        var address = new AddressEntity { Street = "Testowa", Number = "1", City = "Warszawa", PostalCode = "00-100", Country = "Polska" };
        db.Set<AddressEntity>().Add(address);
        await db.SaveChangesAsync();

        var user = new UserEntity { Id = 1, Email = "sprzedawca@test.local", Name = "Sprzedawca", CompanyName = "Testowa Firma", Nip = "1234567890", AddressId = address.AddressId };
        db.Users.Add(user);

        db.Set<ClientEntity>().Add(new ClientEntity { Name = "Klient Testowy", Nip = "1111111111", UserId = 1, AddressId = address.AddressId });
        db.Set<ProductEntity>().Add(new ProductEntity { Name = "Produkt Testowy", Value = 100, UserId = 1 });

        await db.SaveChangesAsync();
    }

    private async Task SeedUserWithoutDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var existingUser = await db.Users.FindAsync(1);
        if (existingUser != null) return;

        var address = new AddressEntity { Street = "Brak Danych", Number = "0", City = "Brak", PostalCode = "00-000", Country = "Polska" };
        db.Set<AddressEntity>().Add(address);
        await db.SaveChangesAsync();

        db.Users.Add(new UserEntity { Id = 1, Email = "pusty@test.local", Name = "Pusty", CompanyName = "Firma", Nip = "0000000000", AddressId = address.AddressId });
        await db.SaveChangesAsync();
    }
}