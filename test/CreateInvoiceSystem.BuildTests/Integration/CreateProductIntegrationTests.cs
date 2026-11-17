using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Shared.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace CreateInvoiceSystem.BuildTests.Integration;

[Collection("Integration tests")]
public class CreateProductIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _integrationTestFixture;
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateProductIntegrationTests(
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
    public async Task Should_CreateProduct_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await SeedUserAsync(cancellationToken);

        var payload = new
        {
            Name = "Produkt Testowy",
            Description = "Opis Produktu",
            Value = 150.50m,
            UserId = 0
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Product/create",
            payload,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            because: body);

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var product = await db.Set<ProductEntity>()
            .FirstOrDefaultAsync(
                product => product.Name == "Produkt Testowy",
                cancellationToken);

        product.Should().NotBeNull();
        product!.Description.Should().Be("Opis Produktu");
        product.Value.Should().Be(150.50m);
        product.UserId.Should().Be(1);
    }

    [Fact]
    public async Task Should_Return400_When_NameIsMissing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await SeedUserAsync(cancellationToken);

        var payload = new
        {
            Name = "",
            Description = "Opis",
            Value = 10.00m,
            UserId = 1
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Product/create",
            payload,
            cancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);
    }

    private async Task SeedUserAsync(
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

        await db.SaveChangesAsync(cancellationToken);
    }
}