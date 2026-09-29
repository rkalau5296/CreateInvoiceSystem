using System;
using System.Threading;
using System.Threading.Tasks;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RefreshToken;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

using User = CreateInvoiceSystem.Modules.Users.Domain.Entities.User;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Users;

public class RefreshTokenHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUserAuthService> _userAuthServiceMock = new();
    private readonly RefreshTokenHandler _sut;

    public RefreshTokenHandlerTests()
    {
        _sut = new RefreshTokenHandler(_userRepositoryMock.Object, _userAuthServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenSessionNotFound()
    {
        // Arrange
        var refreshToken = Guid.NewGuid();
        var request = new RefreshTokenRequest(refreshToken);

        _userRepositoryMock
            .Setup(x => x.GetSessionByTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Sesja jest nieważna.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenSessionIsRevoked()
    {
        // Arrange
        var refreshToken = Guid.NewGuid();
        var request = new RefreshTokenRequest(refreshToken);
        var revokedSession = new UserSession
        {
            UserId = 1,
            RefreshToken = refreshToken,
            IsRevoked = true,
            LastActivityAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(x => x.GetSessionByTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => revokedSession);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Sesja jest nieważna.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenSessionIsOlderThan30Minutes()
    {
        // Arrange
        var refreshToken = Guid.NewGuid();
        var request = new RefreshTokenRequest(refreshToken);

        var expiredSession = new UserSession
        {
            UserId = 1,
            RefreshToken = refreshToken,
            IsRevoked = false,
            LastActivityAt = DateTime.UtcNow.AddMinutes(-31)
        };

        _userRepositoryMock
            .Setup(x => x.GetSessionByTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => expiredSession);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Sesja wygasła z powodu bezczynności.");

        expiredSession.IsRevoked.Should().BeTrue();

        _userRepositoryMock.Verify(x =>
            x.UpdateSessionAsync(expiredSession, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserDoesNotExist()
    {
        // Arrange
        var refreshToken = Guid.NewGuid();
        var request = new RefreshTokenRequest(refreshToken);

        var session = new UserSession
        {
            UserId = 1,
            RefreshToken = refreshToken,
            IsRevoked = false,
            LastActivityAt = DateTime.UtcNow
        };

        _userRepositoryMock
            .Setup(x => x.GetSessionByTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => session);

        _userRepositoryMock
            .Setup(x => x.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Użytkownik nie istnieje.");
    }

    [Fact]
    public async Task Handle_ShouldReturnNewTokensAndUpdateSession_WhenSessionIsActive()
    {
        // Arrange
        var oldRefreshToken = Guid.NewGuid();
        var newRefreshToken = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var request = new RefreshTokenRequest(oldRefreshToken);
        const int userId = 1;
        const string email = "test@example.com";

        var activeSession = new UserSession
        {
            UserId = userId,
            SessionId = sessionId,
            RefreshToken = oldRefreshToken,
            IsRevoked = false,
            LastActivityAt = DateTime.UtcNow.AddMinutes(-2)
        };

        var user = new User
        {
            UserId = userId,
            Email = email
        };

        var expectedResponse = new AuthResponse("new-access-token", newRefreshToken);

        _userRepositoryMock
            .Setup(x => x.GetSessionByTokenAsync(oldRefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => activeSession);

        _userRepositoryMock
            .Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => user);

        _userAuthServiceMock
            .Setup(x => x.GenerateAuthResponse(It.Is<UserAuthModel>(m => m.Id == userId && m.Email == email), sessionId))
            .Returns(expectedResponse);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be(newRefreshToken);

        activeSession.RefreshToken.Should().Be(newRefreshToken);
        activeSession.IsRevoked.Should().BeFalse();

        _userRepositoryMock.Verify(x =>
            x.UpdateSessionAsync(activeSession, It.IsAny<CancellationToken>()),
            Times.Once);

        _userAuthServiceMock.Verify(x => x.GenerateAuthResponse(
            It.Is<UserAuthModel>(m => m.Id == userId && m.Email == email),
            sessionId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenSessionIsActiveLessThan30Minutes()
    {
        // Arrange
        var refreshToken = Guid.NewGuid();
        var newRefreshToken = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var request = new RefreshTokenRequest(refreshToken);

        var activeSession = new UserSession
        {
            UserId = 1,
            SessionId = sessionId,
            RefreshToken = refreshToken,
            IsRevoked = false,
            LastActivityAt = DateTime.UtcNow.AddMinutes(-10)
        };

        var user = new User
        {
            UserId = 1,
            Email = "test@example.com",
            CompanyName = "Test Company",
            Nip = "1234567890",
            IsActive = true
        };

        var authResponse = new AuthResponse(
            "new_access_token",
            newRefreshToken);

        _userRepositoryMock
            .Setup(x => x.GetSessionByTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => activeSession);

        _userRepositoryMock
            .Setup(x => x.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => user);

        _userAuthServiceMock
            .Setup(x => x.GenerateAuthResponse(
                It.IsAny<UserAuthModel>(),
                sessionId))
            .Returns(authResponse);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("new_access_token");
        result.RefreshToken.Should().Be(newRefreshToken);

        activeSession.RefreshToken.Should().Be(newRefreshToken);
        activeSession.SessionId.Should().Be(sessionId);
        activeSession.LastActivityAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromSeconds(2));

        _userAuthServiceMock.Verify(x => x.GenerateAuthResponse(
            It.Is<UserAuthModel>(model =>
                model.Id == user.UserId &&
                model.Email == user.Email),
            sessionId),
            Times.Once);

        _userRepositoryMock.Verify(x =>
            x.UpdateSessionAsync(activeSession, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}