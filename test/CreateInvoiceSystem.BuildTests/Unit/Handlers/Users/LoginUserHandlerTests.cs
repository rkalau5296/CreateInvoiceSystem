using CreateInvoiceSystem.Modules.Users.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.LoginUser;
using CreateInvoiceSystem.Modules.Users.Dto;
using CreateInvoiceSystem.Modules.Users.Entities;
using CreateInvoiceSystem.Modules.Users.Interfaces;
using FluentAssertions;
using Moq;
using User = CreateInvoiceSystem.Modules.Users.Entities.User;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Users;

public class LoginUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUserTokenService> _tokenServiceMock = new();
    private readonly LoginUserHandler _sut;

    public LoginUserHandlerTests()
    {
        _sut = new LoginUserHandler(_userRepositoryMock.Object, _tokenServiceMock.Object);
    }    

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
    {
        // Arrange
        var dto = new LoginUserDto("nonexistent@example.com", "Password123!", true);
        var request = new LoginUserRequest(dto);

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(dto.Email))
            .ReturnsAsync(() => null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Błędny użytkownik lub hasło.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIsNotActive()
    {
        // Arrange
        var dto = new LoginUserDto("inactive@example.com", "Password123!", true);
        var request = new LoginUserRequest(dto);
        var inactiveUser = new User { UserId = 1, Email = dto.Email, IsActive = false };

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(dto.Email))
            .ReturnsAsync(() => inactiveUser);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Konto nie jest aktywne. Sprawdź e-mail, aby dokończyć rejestrację.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenPasswordIsInvalid()
    {
        // Arrange
        var dto = new LoginUserDto("user@example.com", "WrongPassword", true);
        var request = new LoginUserRequest(dto);
        var activeUser = new User { UserId = 1, Email = dto.Email, IsActive = true };

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(dto.Email))
            .ReturnsAsync(() => activeUser);

        _userRepositoryMock
            .Setup(r => r.CheckPasswordAsync(activeUser, dto.Password))
            .ReturnsAsync(() => null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Błędny użytkownik lub hasło.");
    }

    [Fact]
    public async Task Handle_ShouldReturnTokensAndCreateSession_WhenCredentialsAreValid()
    {
        // Arrange
        var dto = new LoginUserDto("user@example.com", "CorrectPassword123!", true);
        var request = new LoginUserRequest(dto);
        var activeUser = new User
        {
            UserId = 1,
            Email = dto.Email,
            CompanyName = "Test Firm",
            Nip = "1234567890",
            IsActive = true
        };
        var roles = new List<string> { "User", "Admin" };
        const string expectedAccessToken = "access-token-123";
        var expectedRefreshToken = Guid.NewGuid();

        _userRepositoryMock
            .Setup(r => r.FindByEmailAsync(dto.Email))
            .ReturnsAsync(() => activeUser);

        _userRepositoryMock
            .Setup(r => r.CheckPasswordAsync(activeUser, dto.Password))
            .ReturnsAsync(() => activeUser);

        _userRepositoryMock
            .Setup(r => r.GetRolesAsync(activeUser, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        _tokenServiceMock
            .Setup(t => t.CreateToken(
                activeUser.UserId,
                activeUser.Email,
                activeUser.CompanyName,
                activeUser.Nip,
                roles,
                It.IsAny<Guid>()))
            .Returns((expectedAccessToken, expectedRefreshToken));

        _userRepositoryMock
            .Setup(r => r.AddSessionAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Token.Should().Be(expectedAccessToken);
        result.RefreshToken.Should().Be(expectedRefreshToken);
        result.Message.Should().Be("Login successful");

        _userRepositoryMock.Verify(
            r => r.AddSessionAsync(
                It.Is<UserSession>(s => s.UserId == activeUser.UserId && s.RefreshToken == expectedRefreshToken),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}