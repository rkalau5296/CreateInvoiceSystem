using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResendToken;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using User = CreateInvoiceSystem.Modules.Users.Domain.Entities.User;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Users;

public class ResendActivationTokenHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUserTokenService> _userTokenServiceMock = new();
    private readonly Mock<IUserEmailSender> _emailSenderMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly ResendActivationTokenHandler _sut;

    public ResendActivationTokenHandlerTests()
    {
        _sut = new ResendActivationTokenHandler(
            _userRepositoryMock.Object,
            _userTokenServiceMock.Object,
            _emailSenderMock.Object,
            _configurationMock.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Handle_ShouldReturnFailure_WhenEmailIsNullOrEmpty(string? email)
    {
        // Arrange
        var request = new ResendActivationTokenRequest { Email = email! };

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Email jest wymagany.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserDoesNotExist_ToPreventEnumerationAttack()
    {
        // Arrange
        const string email = "nonexistent@example.com";
        var request = new ResendActivationTokenRequest { Email = email };

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync((User?)null!);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Jeśli konto istnieje i nie jest aktywne, nowy link został wysłany.");

        _userTokenServiceMock.Verify(t => t.GenerateActivationToken(It.IsAny<string>()), Times.Never);
        _emailSenderMock.Verify(e => e.SendActivationEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsAlreadyActive()
    {
        // Arrange
        const string email = "active@example.com";
        var request = new ResendActivationTokenRequest { Email = email };
        var activeUser = new User { UserId = 1, Email = email, IsActive = true };

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync(activeUser);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("To konto jest już aktywne.");

        _userTokenServiceMock.Verify(t => t.GenerateActivationToken(It.IsAny<string>()), Times.Never);
        _emailSenderMock.Verify(e => e.SendActivationEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenFrontendUrlIsInvalid()
    {
        // Arrange
        const string email = "inactive@example.com";
        var request = new ResendActivationTokenRequest { Email = email };
        var inactiveUser = new User { UserId = 1, Email = email, IsActive = false };

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync(inactiveUser);

        _userTokenServiceMock
            .Setup(t => t.GenerateActivationToken(email))
            .Returns("dummy-token");

        _configurationMock.Setup(c => c["FrontendUrl"]).Returns("invalid-url-format");

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*BŁĄD KONFIGURACJI: 'FrontendUrl' jest nieprawidłowy lub nieobecny*");
    }

    [Fact]
    public async Task Handle_ShouldGenerateToken_SaveJti_AndSendEmail_WhenUserIsInactive()
    {
        // Arrange
        const string email = "user@example.com";
        var request = new ResendActivationTokenRequest { Email = email };
        var inactiveUser = new User { UserId = 5, Email = email, IsActive = false };
                
        const string mockJwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJyZXNlbmQtanRpLTk5OSIsImV4cCI6MTg5MzQ1NjAwMH0.signature";

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(email))
            .ReturnsAsync(inactiveUser);

        _userTokenServiceMock
            .Setup(t => t.GenerateActivationToken(email))
            .Returns(mockJwtToken);

        _configurationMock
            .Setup(c => c["FrontendUrl"])
            .Returns("https://app.example.com/");

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Nowy link aktywacyjny został wysłany na Twój e-mail.");

        _userRepositoryMock.Verify(
            r => r.SaveActivationTokenJtiAsync(inactiveUser.UserId, "resend-jti-999", It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Once);

        var expectedLink = $"https://app.example.com/activate?token={Uri.EscapeDataString(mockJwtToken)}";
        _emailSenderMock.Verify(
            e => e.SendActivationEmailAsync(email, expectedLink),
            Times.Once);
    }
}