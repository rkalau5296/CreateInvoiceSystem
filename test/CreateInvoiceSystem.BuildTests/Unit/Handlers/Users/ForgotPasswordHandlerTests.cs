using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ForgotPassword;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;
using User = CreateInvoiceSystem.Modules.Users.Domain.Entities.User;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Users;

public class ForgotPasswordHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUserEmailSender> _emailSenderMock = new();
    private readonly ForgotPasswordHandler _sut;

    public ForgotPasswordHandlerTests()
    {
        _sut = new ForgotPasswordHandler(_userRepositoryMock.Object, _emailSenderMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessAndSendEmail_WhenUserExistsAndTokenGenerated()
    {
        // Arrange
        const string email = "existing@test.com";
        const string token = "secret-token";
        const string version = "reset-version-123";

        var request = new ForgotPasswordRequest(new ForgotPasswordDto(email));
        var user = new User { UserId = 1, Email = email };

        _userRepositoryMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync((token, version));

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("If your email is in our database, you will receive a reset link.");

        _emailSenderMock.Verify(
            x => x.SendResetPasswordEmailAsync(email, token, version),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessButNotSendEmail_WhenUserDoesNotExist()
    {
        // Arrange
        const string email = "nonexistent@test.com";
        var request = new ForgotPasswordRequest(new ForgotPasswordDto(email));

        _userRepositoryMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((User?)null!);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("If your email is in our database, you will receive a reset link.");

        _emailSenderMock.Verify(
            x => x.SendResetPasswordEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _userRepositoryMock.Verify(
            x => x.GeneratePasswordResetTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessButNotSendEmail_WhenTokenGenerationReturnsNull()
    {
        // Arrange
        const string email = "existing@test.com";
        var request = new ForgotPasswordRequest(new ForgotPasswordDto(email));
        var user = new User { UserId = 1, Email = email };

        _userRepositoryMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((string, string)?)null);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _emailSenderMock.Verify(
            x => x.SendResetPasswordEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }    
}