using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Shared.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace CreateInvoiceSystem.BuildTests.Integration;

[Collection("Integration tests")]
public class DeleteProductIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _integrationTestFixture;
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeleteProductIntegrationTests(
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
    public async Task Should_DeleteProduct_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await SeedUserAsync(cancellationToken);
        var productId = await SeedProductAsync(cancellationToken);

        var response = await _client.DeleteAsync(
            $"/api/Product/{productId}",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            because: body);

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var deletedProduct = await db.Set<ProductEntity>()
            .FirstOrDefaultAsync(
                product => product.ProductId == productId,
                cancellationToken);

        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task Should_Return404_When_ProductDoesNotExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await SeedUserAsync(cancellationToken);

        var response = await _client.DeleteAsync(
            "/api/Product/99999",
            cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Return400_When_IdIsInvalid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await SeedUserAsync(cancellationToken);

        var response = await _client.DeleteAsync(
            "/api/Product/0",
            cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

    private async Task<int> SeedProductAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var product = new ProductEntity
        {
            Name = $"Produkt_{Guid.NewGuid():N}",
            Description = "Do usunięcia",
            Value = 99.99m,
            UserId = 1
        };

        db.Set<ProductEntity>().Add(product);

        await db.SaveChangesAsync(cancellationToken);

        return product.ProductId;
    }
}