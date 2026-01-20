using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RegisterUser;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Abstractions.CQRS;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Users.Handlers;

public class RegisterUserHandlerTests
{
    private readonly Mock<ICommandExecutor> _commandExecutorMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _commandExecutorMock = new Mock<ICommandExecutor>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _handler = new RegisterUserHandler(
            _commandExecutorMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnData_WhenCommandExecutesSuccessfully()
    {
        // Arrange
        var userDto = new RegisterUserDto
        {
            Email = "test@firma.pl",
            Name = "Marek",
            CompanyName = "Firma X"
        };

        var request = new RegisterUserRequest { User = userDto };        
        var expectedResult = new RegisterUserDto
        {
            Email = "test@firma.pl",
            Name = "Marek"
        };
        
        _commandExecutorMock
            .Setup(x => x.Execute<RegisterUserDto, RegisterUserDto, IUserRepository>(
                It.IsAny<CommandBase<RegisterUserDto, RegisterUserDto, IUserRepository>>(),
                It.IsAny<IUserRepository>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Email.Should().Be("test@firma.pl");
        
        _commandExecutorMock.Verify(x => x.Execute<RegisterUserDto, RegisterUserDto, IUserRepository>(
            It.Is<RegisterUserCommand>(c => c.Parametr == userDto),
            _userRepositoryMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHandleDefaultRequest_WithoutCrashing()
    {
        // Arrange
        var request = new RegisterUserRequest(); // User = new() w środku
        var expectedResult = new RegisterUserDto { Email = "default@test.pl" };

        _commandExecutorMock
            .Setup(x => x.Execute<RegisterUserDto, RegisterUserDto, IUserRepository>(
                It.IsAny<CommandBase<RegisterUserDto, RegisterUserDto, IUserRepository>>(),
                It.IsAny<IUserRepository>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Email.Should().Be("default@test.pl");
    }
}