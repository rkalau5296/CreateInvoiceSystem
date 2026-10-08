using CreateInvoiceSystem.BuildTests.Helper;
using CreateInvoiceSystem.Identity.Interfaces;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Shared.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CreateInvoiceSystem.BuildTests.Integration;

[Collection("Integration tests")]
public class AuthControllerIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _integrationTestFixture;
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(
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
    public async Task Should_Return400_When_PasswordsDoNotMatch()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var payload = new
        {
            Dto = new
            {
                OldPassword = "!OldPassword123",
                NewPassword = "!NewPassword123",
                ConfirmPassword = "!DifferentPassword123"
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/change-password",
            payload,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest,
            because: body);
    }

    [Fact]
    public async Task Should_Return400_When_UserNotFoundDuringPasswordChange()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var payload = new
        {
            Dto = new
            {
                OldPassword = "!OldPassword123",
                NewPassword = "!NewPassword456",
                ConfirmPassword = "!NewPassword456"
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/change-password",
            payload,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest,
            because: body);
    }

    [Fact]
    public async Task Should_RegisterUser_When_DataIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var email = $"register_{Guid.NewGuid()}@test.local";

        var payload = new
        {
            User = new
            {
                Email = email,
                Password = "!Password123",
                Name = "New User",
                CompanyName = "New Company",
                Nip = Helpers.CreateTestNip(),
                BankAccountNumber = "",
                Address = new
                {
                    Street = "Testowa",
                    Number = "10",
                    City = "Warszawa",
                    PostalCode = "00-001",
                    Country = "Polska"
                }
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            payload,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            because: body);
    }

    [Fact]
    public async Task Should_LoginUser_When_CredentialsAreCorrect()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var email = $"login_{Guid.NewGuid()}@test.local";
        const string pass = "!Password123";

        await SeedFullUserAsync(
            email: email,
            password: pass,
            isActive: true,
            cancellationToken: cancellationToken);

        var payload = new
        {
            Dto = new
            {
                Email = email,
                Password = pass,
                RememberMe = false
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            payload,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            because: body);
    }

    [Fact]
    public async Task Should_ActivateUser_When_TokenIsValid()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var scope = _factory.Services.CreateScope();

        var jwtProvider = scope.ServiceProvider
            .GetRequiredService<IJwtProvider>();

        var email = $"token_{Guid.NewGuid()}@test.local";
        var token = jwtProvider.GenerateActivationToken(email, 24);
        var (jti, expiry) = ParseJtiAndExpiryFromJwt(token);

        await SeedFullUserAsync(
            email: email,
            password: "AnyPassword123!",
            isActive: false,
            jti: jti,
            expiry: expiry,
            cancellationToken: cancellationToken);

        var response = await _client.GetAsync(
            $"/api/Auth/activate?token={token}",
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            because: body);
    }

    private static (string? jti, DateTimeOffset? expiryUtc)
        ParseJtiAndExpiryFromJwt(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');

            if (parts.Length < 2)
            {
                return (null, null);
            }

            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            switch (payload.Length % 4)
            {
                case 2:
                    payload += "==";
                    break;

                case 3:
                    payload += "=";
                    break;
            }

            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);

            using var document = JsonDocument.Parse(json);

            var root = document.RootElement;

            string? jti = null;
            DateTimeOffset? expiry = null;

            if (root.TryGetProperty("jti", out var jtiProperty))
            {
                jti = jtiProperty.GetString();
            }

            if (root.TryGetProperty("exp", out var expiryProperty))
            {
                expiry = DateTimeOffset.FromUnixTimeSeconds(
                    expiryProperty.GetInt64());
            }

            return (jti, expiry);
        }
        catch (FormatException)
        {
            return (null, null);
        }
        catch (JsonException)
        {
            return (null, null);
        }
    }

    private async Task SeedFullUserAsync(
        string email,
        string password,
        bool isActive = true,
        string? jti = null,
        DateTimeOffset? expiry = null,
        CancellationToken cancellationToken = default)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CreateInvoiceSystemDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<UserEntity>>();

        var existing = await db.Users.FirstOrDefaultAsync(
            user => user.Email == email,
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
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            Name = "Test",
            CompanyName = "Test",
            Nip = $"T{Random.Shared.Next(100000000, 999999999)}",
            IsActive = isActive,
            ActivationTokenJti = jti,
            ActivationTokenExpiry = expiry,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            EmailConfirmed = isActive,
            AddressId = address.AddressId
        };

        var result = await userManager.CreateAsync(
            user,
            password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Failed to create user: " +
                string.Join(
                    ", ",
                    result.Errors.Select(error => error.Description)));
        }

        if (isActive)
        {
            user.IsActive = true;
            user.EmailConfirmed = true;

            await userManager.UpdateAsync(user);
        }
    }
}