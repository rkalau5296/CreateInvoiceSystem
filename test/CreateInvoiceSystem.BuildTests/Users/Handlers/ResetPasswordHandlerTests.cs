using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResetPassword;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Users.Handlers;

public class ResetPasswordHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly ResetPasswordHandler _handler;

    public ResetPasswordHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new ResetPasswordHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPasswordResetSucceeds()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            Token = "valid-token",
            Version = "valid-version",
            NewPassword = "NewStrongPassword123!"
        };

        var user = new User
        {
            UserId = 1,
            Email = request.Email
        };

        _userRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.ResetPasswordAsync(
                user,
                request.Token,
                request.Version,
                request.NewPassword,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Hasło zostało pomyślnie zmienione.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordResetFails()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            Token = "invalid-token",
            Version = "invalid-version",
            NewPassword = "NewStrongPassword123!"
        };

        var user = new User
        {
            UserId = 1,
            Email = request.Email
        };

        _userRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.ResetPasswordAsync(
                user,
                request.Token,
                request.Version,
                request.NewPassword,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Link do resetu hasła jest nieprawidłowy lub wygasł.");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "missing@example.com",
            Token = "some-token",
            Version = "some-version",
            NewPassword = "NewStrongPassword123!"
        };

        _userRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((User)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Użytkownik nie istnieje.");
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Parametr = null
        };

        // Act
        Func<Task> act = async () => await command.Execute(_userRepositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentNullException>()
            .WithParameterName("Parametr");
    }
}