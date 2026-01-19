using CreateInvoiceSystem.Identity.Models;
using CreateInvoiceSystem.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Users.Authentication;

public class JwtProviderTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly JwtProvider _jwtProvider;

    public JwtProviderTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x["Jwt:Key"]).Returns("secret_key_with_at_least_32_characters_long");
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("test-issuer");
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("test-audience");
        _configurationMock.Setup(x => x["Jwt:ExpiryMinutes"]).Returns("60");

        _jwtProvider = new JwtProvider(_configurationMock.Object);
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
        var tokenString = _jwtProvider.Generate(userModel);

        // Assert
        tokenString.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        token.Issuer.Should().Be("test-issuer");
        token.Audiences.Should().Contain("test-audience");

        token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be("123");
        token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value.Should().Be("jan@test.pl");
        token.Claims.First(c => c.Type == "company_name").Value.Should().Be("Test Corp");
        token.Claims.First(c => c.Type == "nip").Value.Should().Be("1234567890");

        var roles = token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        roles.Should().Contain(new[] { "Admin", "User" });
    }

    [Fact]
    public void Generate_ShouldSetCorrectExpirationTime()
    {
        // Arrange
        var userModel = new IdentityUserModel(1, "t@t.pl", "C", "N", new List<string>());

        // Act
        var tokenString = _jwtProvider.Generate(userModel);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);

        // Assert
        token.ValidTo.Should().BeAfter(DateTime.UtcNow);
        token.ValidTo.Should().BeBefore(DateTime.UtcNow.AddMinutes(61));
    }
}