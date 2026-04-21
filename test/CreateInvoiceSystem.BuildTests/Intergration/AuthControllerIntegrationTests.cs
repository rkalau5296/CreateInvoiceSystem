using CreateInvoiceSystem.BuildTests.Infrastructure;
using CreateInvoiceSystem.Identity.Interfaces;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit.Abstractions;

namespace CreateInvoiceSystem.BuildTests.Intergration;

public class AuthControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public AuthControllerIntegrationTests(TestWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        _factory.ResetEmailMock();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Should_Return400_When_PasswordsDoNotMatch()
    {
        var payload = new
        {
            Dto = new
            {
                OldPassword = "!OldPassword123",
                NewPassword = "!NewPassword123",
                ConfirmPassword = "!DifferentPassword123"
            }
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/change-password", payload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because: body);
    }

    [Fact]
    public async Task Should_Return400_When_UserNotFoundDuringPasswordChange()
    {
        var payload = new
        {
            Dto = new
            {
                OldPassword = "!OldPassword123",
                NewPassword = "!NewPassword456",
                ConfirmPassword = "!NewPassword456"
            }
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/change-password", payload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because: body);
    }

    [Fact]
    public async Task Should_RegisterUser_When_DataIsValid()
    {
        var email = $"register_{Guid.NewGuid()}@test.local";

        var payload = new
        {
            User = new
            {
                Email = email,
                Password = "!Password123",
                Name = "New User",
                CompanyName = "New Company",
                Nip = "1234567890",
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

        var response = await _client.PostAsJsonAsync("/api/Auth/register", payload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);
    }

    [Fact]
    public async Task Should_LoginUser_When_CredentialsAreCorrect()
    {
        var email = $"login_{Guid.NewGuid()}@test.local";
        const string pass = "!Password123";

        await SeedFullUserAsync(email: email, password: pass, isActive: true);

        var payload = new
        {
            Dto = new
            {
                Email = email,
                Password = pass,
                RememberMe = false
            }
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", payload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);
    }

    [Fact]
    public async Task Should_ActivateUser_When_TokenIsValid()
    {
        using var scope = _factory.Services.CreateScope();
        var jwtProvider = scope.ServiceProvider.GetRequiredService<IJwtProvider>();

        var email = $"token_{Guid.NewGuid()}@test.local";
        var token = jwtProvider.GenerateActivationToken(email, 24);
        var (jti, expiry) = ParseJtiAndExpiryFromJwt(token);

        await SeedFullUserAsync(email: email, password: "AnyPassword123!", isActive: false, jti: jti, expiry: expiry);

        var response = await _client.GetAsync($"/api/Auth/activate?token={token}");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);
    }

    private static (string? jti, DateTimeOffset? expiryUtc) ParseJtiAndExpiryFromJwt(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }
            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return (
                root.TryGetProperty("jti", out var j) ? j.GetString() : null,
                root.TryGetProperty("exp", out var e) ? DateTimeOffset.FromUnixTimeSeconds(e.GetInt64()) : null
            );
        }
        catch { return (null, null); }
    }

    private async Task SeedFullUserAsync(
        string email,
        string password,
        bool isActive = true,
        string? jti = null,
        DateTimeOffset? expiry = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreateInvoiceSystemDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();

        var existing = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
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
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            Name = "Test",
            CompanyName = "Test",
            Nip = "1234567890",
            IsActive = isActive,
            ActivationTokenJti = jti,
            ActivationTokenExpiry = expiry,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            EmailConfirmed = isActive,
            AddressId = address.AddressId
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        if (isActive)
        {
            user.IsActive = true;
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }
    }
}