using CreateInvoiceSystem.Identity.Models;
using CreateInvoiceSystem.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Identity;

public class JwtProviderTests
{
    private readonly IConfiguration _configuration;
    private readonly JwtProvider _jwtProvider;

    public JwtProviderTests()
    {        
        var testSettings = new Dictionary<string, string>
        {
            {"Jwt:Key", "secret_key_with_at_least_32_characters_long"},
            {"Jwt:Issuer", "CreateInvoiceSystem"},
            {"Jwt:Audience", "CreateInvoiceSystemUsers"},
            {"Jwt:ExpiryMinutes", "5"} 
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(testSettings!)
            .Build();

        _jwtProvider = new JwtProvider(_configuration);
    }

    [Fact]
    public void Generate_ShouldReturnTokenWithValidClaims()
    {
        // Arrange
        var userModel = new IdentityUserModel(
            123,
            "jan@test.pl",
            "Test Corp",
            "1234567890",
            new List<string> { "Admin", "User" }
        );

        // Act
        var result = _jwtProvider.Generate(userModel);

        // Assert
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe(Guid.Empty);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.AccessToken);

        token.Issuer.Should().Be("CreateInvoiceSystem");
        token.Audiences.Should().Contain("CreateInvoiceSystemUsers");

        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "123");
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "jan@test.pl");
        token.Claims.Should().Contain(c => c.Type == "company_name" && c.Value == "Test Corp");
        token.Claims.Should().Contain(c => c.Type == "nip" && c.Value == "1234567890");

        var roles = token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        roles.Should().Contain(new[] { "Admin", "User" });
    }

    [Fact]
    public void Generate_ShouldSetCorrectExpirationTime()
    {
        // Arrange
        var userModel = new IdentityUserModel(1, "t@t.pl", "C", "N", new List<string>());

        // Act
        var result = _jwtProvider.Generate(userModel);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);

        // Assert
        token.ValidTo.Should().BeAfter(DateTime.UtcNow);        
        token.ValidTo.Should().BeBefore(DateTime.UtcNow.AddMinutes(6));
    }
}