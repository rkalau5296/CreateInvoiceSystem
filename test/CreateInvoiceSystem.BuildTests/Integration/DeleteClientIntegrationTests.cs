using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Shared.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace CreateInvoiceSystem.BuildTests.Integration;

[Collection("Integration tests")]
public class DeleteClientIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _integrationTestFixture;
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeleteClientIntegrationTests(
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
    public async Task Should_DeleteClient_When_RequestIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await SeedUserAsync(cancellationToken);
        var clientId = await SeedClientAsync(cancellationToken);

        var response = await _client.DeleteAsync(
            $"/api/Client/{clientId}",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            because: body);

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var deletedClient = await db.Set<ClientEntity>()
            .FirstOrDefaultAsync(
                client => client.ClientId == clientId,
                cancellationToken);

        deletedClient.Should().BeNull();
    }

    [Fact]
    public async Task Should_Return404_When_ClientDoesNotExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await SeedUserAsync(cancellationToken);

        var response = await _client.DeleteAsync(
            "/api/Client/99999",
            cancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Return400_When_IdIsInvalid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await SeedUserAsync(cancellationToken);

        var response = await _client.DeleteAsync(
            "/api/Client/0",
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
            CompanyName = "Testowa Firma Sprzedawcy",
            Nip = "1234567890",
            AddressId = address.AddressId
        };

        db.Users.Add(user);

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> SeedClientAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var address = new AddressEntity
        {
            Street = "Do Usunięcia",
            Number = "1",
            City = "Poznań",
            PostalCode = "60-001",
            Country = "Polska"
        };

        db.Set<AddressEntity>().Add(address);

        await db.SaveChangesAsync(cancellationToken);

        var client = new ClientEntity
        {
            Name = $"Klient_{Guid.NewGuid():N}",
            Nip = GenerateUniqueNip(),
            Email = "delete-test@test.local",
            AddressId = address.AddressId,
            UserId = 1
        };

        db.Set<ClientEntity>().Add(client);

        await db.SaveChangesAsync(cancellationToken);

        return client.ClientId;
    }

    private static string GenerateUniqueNip()
    {
        return Random.Shared
            .NextInt64(1000000000L, 9999999999L)
            .ToString();
    }
}