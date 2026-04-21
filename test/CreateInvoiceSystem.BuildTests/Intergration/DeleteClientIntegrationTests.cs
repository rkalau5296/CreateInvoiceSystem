using CreateInvoiceSystem.BuildTests.Infrastructure;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit.Abstractions;

namespace CreateInvoiceSystem.BuildTests.Intergration;

public class DeleteClientIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public DeleteClientIntegrationTests(TestWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _factory.ResetEmailMock();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Should_DeleteClient_When_RequestIsValid()
    {
        await SeedUserAsync();
        var clientId = await SeedClientAsync();

        var response = await _client.DeleteAsync($"/api/Client/{clientId}");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();

        var deletedClient = await db.Set<ClientEntity>().FirstOrDefaultAsync(c => c.ClientId == clientId);
        deletedClient.Should().BeNull();
    }

    [Fact]
    public async Task Should_Return404_When_ClientDoesNotExist()
    {
        await SeedUserAsync();
        var response = await _client.DeleteAsync("/api/Client/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Return400_When_IdIsInvalid()
    {
        await SeedUserAsync();
        var response = await _client.DeleteAsync("/api/Client/0");

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
            Street = "Do Usunięcia",
            Number = "1",
            City = "Poznań",
            PostalCode = "60-001",
            Country = "Polska"
        };
        db.Set<AddressEntity>().Add(address);
        await db.SaveChangesAsync();

        var client = new ClientEntity
        {
            Name = $"Klient_{Guid.NewGuid():N}",
            Nip = GenerateUniqueNip(),
            Email = "delete-test@test.local",
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
}