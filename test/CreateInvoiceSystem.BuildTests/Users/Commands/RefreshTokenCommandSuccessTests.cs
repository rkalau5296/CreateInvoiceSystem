using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RefreshToken;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Users.Commands
{
    public class RefreshTokenCommandRefreshTokenCommandSuccessTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IUserAuthService> _userAuthServiceMock = new();

        [Fact]
        public async Task ExecuteAsync_ShouldThrowUnauthorizedAccessException_WhenSessionIsOlderThan5Minutes()
        {
            // Arrange
            var refreshToken = Guid.NewGuid();
            var request = new RefreshTokenRequest(refreshToken);

            var expiredSession = new UserSession
            {
                UserId = 1,
                RefreshToken = refreshToken,
                IsRevoked = false,
                LastActivityAt = DateTime.UtcNow.AddMinutes(-6) 
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
        public async Task ExecuteAsync_ShouldReturnNewTokens_WhenSessionIsActive()
        {
            // Arrange
            var oldRefreshToken = Guid.NewGuid();
            var newRefreshToken = Guid.NewGuid();
            var request = new RefreshTokenRequest(oldRefreshToken);
            var userId = 1;
            var email = "test@example.com";

            var activeSession = new UserSession
            {
                UserId = userId,
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
                .ReturnsAsync(activeSession);

            _userRepositoryMock
                .Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userAuthServiceMock
                .Setup(x => x.GenerateAuthResponse(It.Is<UserAuthModel>(m => m.Id == userId && m.Email == email)))
                .Returns(expectedResponse);

            var command = new RefreshTokenCommand(
                request,
                _userRepositoryMock.Object,
                _userAuthServiceMock.Object);

            // Act
            var result = await command.ExecuteAsync(CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().Be("new-access-token");
            result.RefreshToken.Should().Be(newRefreshToken);
                        
            _userRepositoryMock.Verify(x =>
                x.UpdateSessionAsync(activeSession, It.IsAny<CancellationToken>()),
                Times.Once);
            activeSession.IsRevoked.Should().BeFalse();
        }
    }
}
