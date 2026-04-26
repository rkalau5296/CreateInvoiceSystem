using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RefreshToken;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;


namespace CreateInvoiceSystem.BuildTests.Unit
{
    public class RefreshTokenCommandTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IUserAuthService> _userAuthServiceMock = new();

        [Fact]
        public async Task ExecuteAsync_ShouldThrowUnauthorizedAccessException_WhenSessionIsOlderThan30Minutes()
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
                .ReturnsAsync(expiredSession);

            var command = new RefreshTokenCommand(
                request,
                _userRepositoryMock.Object,
                _userAuthServiceMock.Object);

            // Act
            Func<Task> act = async () => await command.ExecuteAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Sesja wygasła z powodu bezczynności.");

            expiredSession.IsRevoked.Should().BeTrue();

            _userRepositoryMock.Verify(x =>
                x.UpdateSessionAsync(expiredSession, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldSucceed_WhenSessionIsActiveLessThan30Minutes()
        {
            // Arrange
            var refreshToken = Guid.NewGuid();
            var newRefreshToken = Guid.NewGuid();
            var request = new RefreshTokenRequest(refreshToken);

            var activeSession = new UserSession
            {
                UserId = 1,
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
                newRefreshToken
            );

            _userRepositoryMock
                .Setup(x => x.GetSessionByTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeSession);

            _userRepositoryMock
                .Setup(x => x.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userAuthServiceMock
                .Setup(x => x.GenerateAuthResponse(It.IsAny<UserAuthModel>()))
                .Returns(authResponse);

            var command = new RefreshTokenCommand(
                request,
                _userRepositoryMock.Object,
                _userAuthServiceMock.Object);

            // Act
            var result = await command.ExecuteAsync(CancellationToken.None);

            // Assert
            result.AccessToken.Should().Be("new_access_token");
            result.RefreshToken.Should().Be(newRefreshToken);

            activeSession.LastActivityAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

            _userRepositoryMock.Verify(x =>
                x.UpdateSessionAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
