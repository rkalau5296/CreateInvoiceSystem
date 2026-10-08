using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Shared.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace CreateInvoiceSystem.BuildTests.Integration;

[Collection("Integration tests")]
public class ExportControllerIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _integrationTestFixture;
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ExportControllerIntegrationTests(
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

    [Theory]
    [InlineData("/api/export/invoices", "faktury.csv")]
    [InlineData("/api/export/products", "produkty.csv")]
    [InlineData("/api/export/clients", "klienci.csv")]
    public async Task Should_DownloadCsv_When_UserIsAuthenticated(
        string url,
        string expectedFileName)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await SeedUserAndDataAsync(cancellationToken);

        var response = await _client.GetAsync(
            url,
            cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType
            .Should()
            .Be("text/csv");

        response.Content.Headers.ContentDisposition?.FileName
            .Should()
            .Be(expectedFileName);

        var content = await response.Content.ReadAsByteArrayAsync(
            cancellationToken);

        content.Length.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("/api/export/invoices")]
    [InlineData("/api/export/products")]
    [InlineData("/api/export/clients")]
    public async Task Should_ReturnEmptyCsv_When_UserHasNoData(
        string url)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await SeedUserWithoutDataAsync(cancellationToken);

        var response = await _client.GetAsync(
            url,
            cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsByteArrayAsync(
            cancellationToken);

        content.Length.Should().BeGreaterThan(0);
    }

    private async Task SeedUserAndDataAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var existingUser = await db.Users.FindAsync(
            new object[] { 1 },
            cancellationToken);

        if (existingUser is not null)
        {
            return;
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
            CompanyName = "Testowa Firma",
            Nip = "1234567890",
            AddressId = address.AddressId
        };

        db.Users.Add(user);

        db.Set<ClientEntity>().Add(
            new ClientEntity
            {
                Name = "Klient Testowy",
                Nip = "1111111111",
                UserId = 1,
                AddressId = address.AddressId
            });

        db.Set<ProductEntity>().Add(
            new ProductEntity
            {
                Name = "Produkt Testowy",
                Value = 100,
                UserId = 1
            });

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedUserWithoutDataAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var existingUser = await db.Users.FindAsync(
            new object[] { 1 },
            cancellationToken);

        if (existingUser is not null)
        {
            return;
        }

        var address = new AddressEntity
        {
            Street = "Brak Danych",
            Number = "0",
            City = "Brak",
            PostalCode = "00-000",
            Country = "Polska"
        };

        db.Set<AddressEntity>().Add(address);

        await db.SaveChangesAsync(cancellationToken);

        db.Users.Add(
            new UserEntity
            {
                Email = "pusty@test.local",
                Name = "Pusty",
                CompanyName = "Firma",
                Nip = "0000000000",
                AddressId = address.AddressId
            });

        await db.SaveChangesAsync(cancellationToken);
    }
}