using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RegisterUser;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using User = CreateInvoiceSystem.Modules.Users.Domain.Entities.User;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Users;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUserEmailSender> _emailSenderMock = new();
    private readonly Mock<IUserTokenService> _userTokenServiceMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly RegisterUserHandler _sut;

    public RegisterUserHandlerTests()
    {
        _sut = new RegisterUserHandler(
            _userRepositoryMock.Object,
            _emailSenderMock.Object,
            _userTokenServiceMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenUserDtoIsNull()
    {
        // Arrange
        var request = new RegisterUserRequest { User = null! };

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("User");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenUserCreationFails()
    {
        // Arrange
        var userDto = new RegisterUserDto
        {
            Email = "duplicate@example.com",
            Password = "Password123!"
        };
        var request = new RegisterUserRequest { User = userDto };

        var identityErrors = new[]
        {
            new IdentityError { Code = "DuplicateEmail", Description = "Email already in use" }
        };

        _userRepositoryMock
            .Setup(r => r.CreateWithPasswordAsync(It.IsAny<User>(), userDto.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Rejestracja nieudana: Podany adres e‑mail jest już używany.*");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenFrontendUrlIsMissingOrInvalid()
    {
        // Arrange
        var userDto = new RegisterUserDto
        {
            Email = "test@example.com",
            Password = "Password123!"
        };
        var request = new RegisterUserRequest { User = userDto };

        _userRepositoryMock
            .Setup(r => r.CreateWithPasswordAsync(It.IsAny<User>(), userDto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userTokenServiceMock
            .Setup(t => t.GenerateActivationToken(It.IsAny<string>()))
            .Returns("dummy-token");

        _configurationMock.Setup(c => c["FrontendUrl"]).Returns((string?)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*BŁĄD KONFIGURACJI: 'FrontendUrl' jest nieprawidłowy lub nieobecny*");
    }

    [Fact]
    public async Task Handle_ShouldRegisterUser_AndSendActivationEmail_WhenDataIsValid()
    {
        // Arrange
        var userDto = new RegisterUserDto
        {
            Email = "jan.kowalski@example.com",
            Password = "Password123!",
            Name = "Jan Kowalski",            
            CompanyName = "Test Firm",
            Nip = "1234567890"
        };
        var request = new RegisterUserRequest { User = userDto };
                
        const string mockJwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJ0ZXN0LWp0aS0xMjMiLCJleHAiOjE4OTM0NTYwMDB9.signature";

        _userRepositoryMock
            .Setup(r => r.CreateWithPasswordAsync(It.IsAny<User>(), userDto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userTokenServiceMock
            .Setup(t => t.GenerateActivationToken(userDto.Email))
            .Returns(mockJwtToken);

        var existingUser = new User
        {
            UserId = 10,
            Email = userDto.Email,
            Name = userDto.Name,            
            CompanyName = userDto.CompanyName,
            Nip = userDto.Nip
        };

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(userDto.Email))
            .ReturnsAsync(existingUser);

        _configurationMock
            .Setup(c => c["FrontendUrl"])
            .Returns("https://app.example.com");

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Email.Should().Be(userDto.Email);
        result.Data.Name.Should().Be(userDto.Name);

        _userRepositoryMock.Verify(
            r => r.SaveActivationTokenJtiAsync(existingUser.UserId, "test-jti-123", It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Once);

        var expectedActivationLink = $"https://app.example.com/activate?token={Uri.EscapeDataString(mockJwtToken)}";
        _emailSenderMock.Verify(
            e => e.SendActivationEmailAsync(userDto.Email, expectedActivationLink),
            Times.Once);
    }
}