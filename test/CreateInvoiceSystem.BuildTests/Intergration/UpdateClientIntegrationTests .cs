using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace CreateInvoiceSystem.BuildTests.Intergration;

public class UpdateClientIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public UpdateClientIntegrationTests(TestWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _factory.ResetEmailMock();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Should_UpdateClient_When_RequestIsValid()
    {
        await SeedUserAsync();
        var clientId = await SeedClientAsync();

        var payload = BuildUpdatePayload(clientId, name: "Zaktualizowany Klient", email: "nowy@test.local");

        var response = await _client.PutAsJsonAsync($"/api/Client/update/{clientId}", payload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var updated = await db.Set<ClientEntity>().FirstOrDefaultAsync(c => c.ClientId == clientId);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Zaktualizowany Klient");
        updated.Email.Should().Be("nowy@test.local");
    }

    [Fact]
    public async Task Should_UpdateClientAddress_When_RequestIsValid()
    {
        await SeedUserAsync();
        var clientId = await SeedClientAsync();

        var payload = BuildUpdatePayload(clientId, street: "Nowa Ulica", city: "Kraków", postalCode: "30-001");

        var response = await _client.PutAsJsonAsync($"/api/Client/update/{clientId}", payload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var updated = await db.Set<ClientEntity>().FirstOrDefaultAsync(c => c.ClientId == clientId);
        updated.Should().NotBeNull();

        var address = await db.Set<AddressEntity>().FirstOrDefaultAsync(a => a.AddressId == updated!.AddressId);
        address.Should().NotBeNull();
        address!.Street.Should().Be("Nowa Ulica");
        address.City.Should().Be("Kraków");
        address.PostalCode.Should().Be("30-001");
    }

    [Fact]
    public async Task Should_Return404_When_ClientDoesNotExist()
    {
        await SeedUserAsync();
        var payload = BuildUpdatePayload(99999);

        var response = await _client.PutAsJsonAsync("/api/Client/update/99999", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Return400_When_NameIsMissing()
    {
        await SeedUserAsync();
        var clientId = await SeedClientAsync();
        var payload = BuildUpdatePayload(clientId, name: "");

        var response = await _client.PutAsJsonAsync($"/api/Client/update/{clientId}", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_When_NipIsMissing()
    {
        await SeedUserAsync();
        var clientId = await SeedClientAsync();
        var payload = BuildUpdatePayload(clientId, nip: "");

        var response = await _client.PutAsJsonAsync($"/api/Client/update/{clientId}", payload);

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
            CompanyName = "Testowa Firma Sprzedawcy",
            Nip = "1234567890",
            AddressId = address.AddressId
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    private async Task<int> SeedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

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

        var client = new ClientEntity
        {
            Name = $"Klient_{Guid.NewGuid():N}",
            Nip = GenerateUniqueNip(),
            Email = "klient@test.local",
            AddressId = address.AddressId,
            UserId = 1
        };
        db.Set<ClientEntity>().Add(client);
        await db.SaveChangesAsync();

        return client.ClientId;
    }

    private static string GenerateUniqueNip()
    {
        var random = new Random();
        return random.NextInt64(1000000000L, 9999999999L).ToString();
    }

    private static object BuildUpdatePayload(
        int clientId,
        string name = "Zaktualizowany Klient",
        string? nip = null,
        string email = "klient@test.local",
        string street = "Testowa",
        string city = "Warszawa",
        string postalCode = "00-100")
    {
        return new
        {
            ClientId = clientId,
            Name = name,
            Nip = nip ?? GenerateUniqueNip(),
            Email = email,
            UserId = 1,
            AddressId = 0,
            Address = new
            {
                AddressId = 0,
                Street = street,
                Number = "1",
                City = city,
                PostalCode = postalCode,
                Country = "Polska"
            }
        };
    }
}