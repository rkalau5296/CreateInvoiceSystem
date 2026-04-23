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

public class CreateClientIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public CreateClientIntegrationTests(TestWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _factory.ResetEmailMock();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Should_CreateClient_And_SaveToDatabase_When_RequestIsValid()
    {
        await SeedUserAsync();
        var uniqueName = $"Klient_{Guid.NewGuid():N}";
        var payload = BuildCreateClientPayload(name: uniqueName);

        var response = await _client.PostAsJsonAsync("/api/Client/create", payload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var saved = await db.Set<ClientEntity>()
            .FirstOrDefaultAsync(c => c.Name == uniqueName);

        saved.Should().NotBeNull();
        saved!.Name.Should().Be(uniqueName);
        saved.Email.Should().Be("klient@test.local");
    }

    [Fact]
    public async Task Should_CreateClient_WithCorrectAddress()
    {
        await SeedUserAsync();
        var uniqueName = $"Klient_{Guid.NewGuid():N}";
        var payload = BuildCreateClientPayload(name: uniqueName);

        var response = await _client.PostAsJsonAsync("/api/Client/create", payload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var saved = await db.Set<ClientEntity>()
            .FirstOrDefaultAsync(c => c.Name == uniqueName);

        saved.Should().NotBeNull();
        saved!.AddressId.Should().BeGreaterThan(0);

        var address = await db.Set<AddressEntity>()
            .FirstOrDefaultAsync(a => a.AddressId == saved.AddressId);

        address.Should().NotBeNull();
        address!.Street.Should().Be("Testowa");
        address.City.Should().Be("Warszawa");
        address.PostalCode.Should().Be("00-100");
    }

    [Fact]
    public async Task Should_Return400_When_NameIsMissing()
    {
        await SeedUserAsync();
        var payload = BuildCreateClientPayload(name: "");

        var response = await _client.PostAsJsonAsync("/api/Client/create", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_When_NipIsMissing()
    {
        await SeedUserAsync();
        var payload = BuildCreateClientPayload(nip: "");

        var response = await _client.PostAsJsonAsync("/api/Client/create", payload);

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

    private static string GenerateUniqueNip()
    {
        var random = new Random();
        return random.NextInt64(1000000000L, 9999999999L).ToString();
    }

    private static object BuildCreateClientPayload(
        string? name = null,
        string? nip = null,
        string email = "klient@test.local")
    {
        return new
        {
            Name = name ?? "Testowy Klient",
            Nip = nip ?? GenerateUniqueNip(),
            Email = email,
            UserId = 1,
            Address = new
            {
                Street = "Testowa",
                Number = "1",
                City = "Warszawa",
                PostalCode = "00-100",
                Country = "Polska"
            }
        };
    }
}