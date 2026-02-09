using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.LoginUser;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Users.Handlers;

public class LoginUserHandlerTests
{
    private readonly Mock<ICommandExecutor> _commandExecutorMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserTokenService> _tokenServiceMock;
    private readonly LoginUserHandler _handler;

    public LoginUserHandlerTests()
    {
        _commandExecutorMock = new Mock<ICommandExecutor>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<IUserTokenService>();

        _handler = new LoginUserHandler(
            _commandExecutorMock.Object,
            _userRepositoryMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnToken_WhenLoginIsSuccessful()
    {
        // Arrange
        var dto = new LoginUserDto("test@example.com", "Password123!", true);
        var request = new LoginUserRequest(dto);
        var expectedAccessToken = "generated-jwt-token";
        var expectedRefreshToken = Guid.NewGuid();
        var expectedResult = new UserTokenResult(expectedAccessToken, expectedRefreshToken);

        _commandExecutorMock.Setup(x => x.Execute(
                It.IsAny<LoginUserCommand>(),
                _userRepositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(expectedAccessToken);
        result.RefreshToken.Should().Be(expectedRefreshToken);
        result.IsSuccess.Should().BeTrue();

        _commandExecutorMock.Verify(x => x.Execute(
            It.IsAny<LoginUserCommand>(),
            _userRepositoryMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTokenIsEmpty()
    {
        // Arrange
        var request = new LoginUserRequest(new LoginUserDto("wrong@test.com", "wrong", true));
        var emptyResult = new UserTokenResult(string.Empty, Guid.Empty);

        _commandExecutorMock.Setup(x => x.Execute(
                It.IsAny<LoginUserCommand>(),
                _userRepositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Token.Should().BeEmpty();
        result.RefreshToken.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenExecutorFails()
    {
        // Arrange
        var request = new LoginUserRequest(new LoginUserDto("error@test.com", "pass", true));

        _commandExecutorMock.Setup(x => x.Execute(
                It.IsAny<LoginUserCommand>(),
                _userRepositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception("Invalid credentials"));

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<System.Exception>().WithMessage("Invalid credentials");
    }
}