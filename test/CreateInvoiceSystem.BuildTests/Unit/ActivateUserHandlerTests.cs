using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ActivateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Text;
using User = CreateInvoiceSystem.Modules.Users.Domain.Entities.User;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class ActivateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserTokenService> _tokenServiceMock;
    private readonly ActivateUserHandler _sut;

    public ActivateUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<IUserTokenService>();
        _sut = new ActivateUserHandler(_userRepositoryMock.Object, _tokenServiceMock.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldReturnError_WhenTokenIsNullOrEmpty(string? token)
    {
        // Arrange
        var request = new ActivateUserRequest { Token = token! };

        // Act
        var response = await _sut.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("Błąd: Brak tokena aktywacyjnego.");
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenTokenServiceReturnsNullEmail()
    {
        // Arrange
        var token = CreateToken("jti-123", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds());
        var request = new ActivateUserRequest { Token = token };

        _tokenServiceMock
            .Setup(s => s.GetEmailFromActivationToken(token))
            .Returns((string?)null);

        // Act
        var response = await _sut.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("Błąd: Link wygasł lub jest nieprawidłowy.");
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenTokenHasNoJti()
    {
        // Arrange
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var tokenWithoutJti = CreateToken(null, exp);
        var request = new ActivateUserRequest { Token = tokenWithoutJti };

        _tokenServiceMock
            .Setup(s => s.GetEmailFromActivationToken(tokenWithoutJti))
            .Returns("test@example.com");

        // Act
        var response = await _sut.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("Błąd: Link wygasł lub jest nieprawidłowy.");
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenTokenIsExpired()
    {
        // Arrange
        var expiredExp = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds();
        var expiredToken = CreateToken("jti-123", expiredExp);
        var request = new ActivateUserRequest { Token = expiredToken };

        _tokenServiceMock
            .Setup(s => s.GetEmailFromActivationToken(expiredToken))
            .Returns("test@example.com");

        // Act
        var response = await _sut.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("Błąd: Link wygasł lub jest nieprawidłowy.");
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenUserDoesNotExist()
    {
        // Arrange
        const string email = "nonexistent@example.com";
        var token = CreateToken("jti-123", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds());
        var request = new ActivateUserRequest { Token = token };

        _tokenServiceMock
            .Setup(s => s.GetEmailFromActivationToken(token))
            .Returns(email);

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync(() => null!);

        // Act
        var response = await _sut.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("Błąd: Użytkownik nie istnieje.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserIsAlreadyActive()
    {
        // Arrange
        const string email = "active@example.com";
        var token = CreateToken("jti-123", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds());
        var request = new ActivateUserRequest { Token = token };

        _tokenServiceMock
            .Setup(s => s.GetEmailFromActivationToken(token))
            .Returns(email);

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync(() => new User { Email = email, IsActive = true });

        // Act
        var response = await _sut.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().Be("Konto jest już aktywne.");

        _userRepositoryMock.Verify(
            r => r.ValidateAndActivateUserByTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenTokenValidationInRepositoryFails()
    {
        // Arrange
        const string email = "user@example.com";
        var token = CreateToken("jti-123", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds());
        var request = new ActivateUserRequest { Token = token };

        _tokenServiceMock
            .Setup(s => s.GetEmailFromActivationToken(token))
            .Returns(email);

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync(() => new User { Email = email, IsActive = false });

        _userRepositoryMock
            .Setup(r => r.ValidateAndActivateUserByTokenAsync(email, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var response = await _sut.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("Błąd: Link wygasł lub jest nieprawidłowy.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenActivationSucceeds()
    {
        // Arrange
        const string email = "user@example.com";
        var token = CreateToken("jti-123", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds());
        var request = new ActivateUserRequest { Token = token };

        _tokenServiceMock
            .Setup(s => s.GetEmailFromActivationToken(token))
            .Returns(email);

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync(() => new User { Email = email, IsActive = false });

        _userRepositoryMock
            .Setup(r => r.ValidateAndActivateUserByTokenAsync(email, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var response = await _sut.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().Be("Konto zostało aktywowane!");

        _userRepositoryMock.Verify(
            r => r.ValidateAndActivateUserByTokenAsync(email, "jti-123", It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static string CreateToken(string? jti, long? exp)
    {
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));

        var payloadDict = new List<string>();
        if (jti != null) payloadDict.Add($"\"jti\":\"{jti}\"");
        if (exp != null) payloadDict.Add($"\"exp\":{exp}");

        var payloadJson = "{" + string.Join(",", payloadDict) + "}";
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson));

        return $"{header}.{payload}.signature";
    }
}