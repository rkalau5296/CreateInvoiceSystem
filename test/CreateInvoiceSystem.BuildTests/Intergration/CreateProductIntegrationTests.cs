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

public class CreateProductIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public CreateProductIntegrationTests(TestWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _factory.ResetEmailMock();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Should_CreateProduct_When_RequestIsValid()
    {
        await SeedUserAsync();
        var payload = new
        {
            Name = "Produkt Testowy",
            Description = "Opis Produktu",
            Value = 150.50m,
            UserId = 0
        };

        var response = await _client.PostAsJsonAsync("/api/Product/create", payload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var product = await db.Set<ProductEntity>().FirstOrDefaultAsync(p => p.Name == "Produkt Testowy");
        product.Should().NotBeNull();
        product!.Description.Should().Be("Opis Produktu");
        product.Value.Should().Be(150.50m);
        product.UserId.Should().Be(1);
    }

    [Fact]
    public async Task Should_Return400_When_NameIsMissing()
    {
        await SeedUserAsync();
        var payload = new
        {
            Name = "",
            Description = "Opis",
            Value = 10.00m,
            UserId = 1
        };

        var response = await _client.PostAsJsonAsync("/api/Product/create", payload);

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
}