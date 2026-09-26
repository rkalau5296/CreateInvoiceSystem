using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ChangePassword;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;
using User = CreateInvoiceSystem.Modules.Users.Domain.Entities.User;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class ChangePasswordHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly ChangePasswordHandler _sut;

    public ChangePasswordHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _sut = new ChangePasswordHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenNewPasswordAndConfirmPasswordDoNotMatch()
    {
        // Arrange
        var dto = new ChangePasswordDto("OldPassword123!", "NewPassword123!", "DifferentPassword123!");
        var request = new ChangePasswordRequest(dto);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Nowe hasło i potwierdzenie nie są zgodne.");

        _userRepositoryMock.Verify(r => r.GetLoggedUserId(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenUserIsNotAuthorized()
    {
        // Arrange
        var request = CreateValidRequest();

        _userRepositoryMock
            .Setup(r => r.GetLoggedUserId(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Nieautoryzowany dostęp.");

        _userRepositoryMock.Verify(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenUserDoesNotExist()
    {
        // Arrange
        const int userId = 10;
        var request = CreateValidRequest();

        _userRepositoryMock
            .Setup(r => r.GetLoggedUserId(It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null!);


        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Użytkownik nie istnieje.");

        _userRepositoryMock.Verify(
            r => r.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenChangePasswordInRepositoryFails()
    {
        // Arrange
        const int userId = 10;
        const string errorMessage = "Obecne hasło jest nieprawidłowe.";
        var request = CreateValidRequest();
        var user = new User { UserId = userId, Email = "user@example.com" };

        _userRepositoryMock
            .Setup(r => r.GetLoggedUserId(It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.ChangePasswordAsync(user, request.Dto.OldPassword, request.Dto.NewPassword))
            .ReturnsAsync((Succeeded: false, ErrorMessage: errorMessage));

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be(errorMessage);

        _userRepositoryMock.Verify(
            r => r.ChangePasswordAsync(user, request.Dto.OldPassword, request.Dto.NewPassword),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPasswordIsChangedSuccessfully()
    {
        // Arrange
        const int userId = 10;
        const string successMessage = "Hasło zostało pomyślnie zmienione.";
        var request = CreateValidRequest();
        var user = new User { UserId = userId, Email = "user@example.com" };

        _userRepositoryMock
            .Setup(r => r.GetLoggedUserId(It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.ChangePasswordAsync(user, request.Dto.OldPassword, request.Dto.NewPassword))
            .ReturnsAsync((Succeeded: true, ErrorMessage: successMessage));

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be(successMessage);

        _userRepositoryMock.Verify(
            r => r.ChangePasswordAsync(user, request.Dto.OldPassword, request.Dto.NewPassword),
            Times.Once);
    }

    private static ChangePasswordRequest CreateValidRequest()
    {
        var dto = new ChangePasswordDto("CurrentPassword123!", "NewSecretPassword123!", "NewSecretPassword123!");
        return new ChangePasswordRequest(dto);
    }
}