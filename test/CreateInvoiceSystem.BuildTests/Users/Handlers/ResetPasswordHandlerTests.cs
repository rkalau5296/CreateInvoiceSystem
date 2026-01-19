using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResetPassword; 
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

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
    public async Task Handle_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "user@test.pl",
            Token = "secret-token",
            NewPassword = "NewStrongPassword123!"
        };

        var user = new User { Email = request.Email };
        
        _userRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);
        
        _userRepositoryMock
            .Setup(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Hasło zostało pomyślnie zmienione.");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new ResetPasswordRequest { Email = "notfound@test.pl" };
        
        _userRepositoryMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((User)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Użytkownik nie istnieje.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenResetPasswordAsyncReturnsFalse()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "user@test.pl",
            Token = "wrong-token",
            NewPassword = "pass"
        };

        var user = new User { Email = request.Email };

        _userRepositoryMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        
        _userRepositoryMock
            .Setup(x => x.ResetPasswordAsync(user, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Błąd resetowania hasła.");
    }
}