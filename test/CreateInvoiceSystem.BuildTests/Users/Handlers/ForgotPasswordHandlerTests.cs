using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ForgotPassword;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Users.Handlers;

public class ForgotPasswordHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserEmailSender> _emailSenderMock;
    private readonly ForgotPasswordHandler _handler;

    public ForgotPasswordHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailSenderMock = new Mock<IUserEmailSender>();
        _handler = new ForgotPasswordHandler(_userRepositoryMock.Object, _emailSenderMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessAndSendEmail_WhenUserExists()
    {
        // Arrange
        var email = "existing@test.com";
        var request = new ForgotPasswordRequest(new ForgotPasswordDto(email));
        var user = new User { UserId = 1, Email = email };
        var token = "secret-token";

        _userRepositoryMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _userRepositoryMock.Setup(x => x.GeneratePasswordResetTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("If your email is in our database, you will receive a reset link.");
        
        _emailSenderMock.Verify(x => x.SendResetPasswordEmailAsync(email, token), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessButNotSendEmail_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "nonexistent@test.com";
        var request = new ForgotPasswordRequest(new ForgotPasswordDto(email));

        _userRepositoryMock.Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((User)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); 
        _emailSenderMock.Verify(x => x.SendResetPasswordEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenDtoIsNull()
    {
        // Arrange
        var request = new ForgotPasswordRequest(null); // Parametr is null

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("Parametr");
    }
}