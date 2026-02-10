using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RefreshToken;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;


namespace CreateInvoiceSystem.BuildTests.Users.Commands
{
    public class RefreshTokenCommandTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IUserAuthService> _userAuthServiceMock = new();

        [Fact]
        public async Task ExecuteAsync_ShouldThrowUnauthorizedAccessException_WhenSessionIsOlderThan15Minutes()
        {
            // Arrange
            var refreshToken = Guid.NewGuid();
            var request = new RefreshTokenRequest(refreshToken);

            var expiredSession = new UserSession
            {
                UserId = 1,
                RefreshToken = refreshToken,
                IsRevoked = false,                
                LastActivityAt = DateTime.UtcNow.AddMinutes(-16)
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
    }
}
