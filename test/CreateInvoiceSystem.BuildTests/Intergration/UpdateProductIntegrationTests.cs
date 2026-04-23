using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace CreateInvoiceSystem.BuildTests.Intergration;

public class UpdateProductIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public UpdateProductIntegrationTests(TestWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _factory.ResetEmailMock();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Should_UpdateProduct_When_RequestIsValid()
    {
        await SeedUserAsync();
        var productId = await SeedProductAsync();

        var payload = new
        {
            ProductId = productId,
            Name = "Zaktualizowana Nazwa",
            Description = "Zaktualizowany Opis",
            Value = 299.99m,
            UserId = 1
        };

        var response = await _client.PutAsJsonAsync($"/api/Product/update/{productId}", payload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var updated = await db.Set<ProductEntity>().FirstOrDefaultAsync(p => p.ProductId == productId);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Zaktualizowana Nazwa");
        updated.Description.Should().Be("Zaktualizowany Opis");
        updated.Value.Should().Be(299.99m);
    }

    [Fact]
    public async Task Should_Return404_When_ProductDoesNotExist()
    {
        await SeedUserAsync();
        var payload = new
        {
            ProductId = 99999,
            Name = "Nieistniejący",
            Description = "Brak",
            Value = 1.00m,
            UserId = 1
        };

        var response = await _client.PutAsJsonAsync("/api/Product/update/99999", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Return400_When_IdInRouteIsInvalid()
    {
        await SeedUserAsync();
        var payload = new
        {
            ProductId = 0,
            Name = "Błędne ID",
            Description = "Opis",
            Value = 10.00m,
            UserId = 1
        };

        var response = await _client.PutAsJsonAsync("/api/Product/update/0", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var existing = await db.Users.FindAsync(1);
        if (existing != null) return;

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
            CompanyName = "Testowa Firma",
            Nip = "1234567890",
            AddressId = address.AddressId
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    private async Task<int> SeedProductAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var product = new ProductEntity
        {
            Name = "Oryginalna Nazwa",
            Description = "Oryginalny Opis",
            Value = 100.00m,
            UserId = 1
        };
        db.Set<ProductEntity>().Add(product);
        await db.SaveChangesAsync();

        return product.ProductId;
    }
}