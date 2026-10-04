using CreateInvoiceSystem.Modules.Users.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.GetUser;
using CreateInvoiceSystem.Modules.Users.Entities;
using CreateInvoiceSystem.Modules.Users.Interfaces;
using FluentAssertions;
using Moq;
using User = CreateInvoiceSystem.Modules.Users.Entities.User;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Users;

public class GetUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly GetUserHandler _sut;

    public GetUserHandlerTests()
    {
        _sut = new GetUserHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnGetUserResponseWithUserData_WhenUserExists()
    {
        // Arrange
        var userId = 1;
        var request = new GetUserRequest(userId);

        var userInDb = new User
        {
            UserId = userId,
            Name = "Jan Kowalski",
            CompanyName = "Januszex Sp. z o.o.",
            Email = "jan@kowalski.pl",
            Nip = "1234567890",
            BankAccountNumber = "PL12345678901234567890123456",
            Address = new Address
            {
                AddressId = 10,
                Street = "Prosta",
                Number = "5",
                City = "Warszawa",
                PostalCode = "00-001",
                Country = "Polska"
            }
        };

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userInDb);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.UserId.Should().Be(userId);
        result.Data.Name.Should().Be("Jan Kowalski");
        result.Data.CompanyName.Should().Be("Januszex Sp. z o.o.");
        result.Data.Email.Should().Be("jan@kowalski.pl");
        result.Data.Address.Should().NotBeNull();
        result.Data.Address!.Street.Should().Be("Prosta");

        _userRepositoryMock.Verify(
            r => r.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenUserDoesNotExist()
    {
        // Arrange
        var nonExistingUserId = 99;
        var request = new GetUserRequest(nonExistingUserId);

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(nonExistingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"User with ID {nonExistingUserId} not found.");

        _userRepositoryMock.Verify(
            r => r.GetUserByIdAsync(nonExistingUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var userId = 1;
        var request = new GetUserRequest(userId);
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        var userInDb = new User
        {
            UserId = userId,
            Name = "Anna Nowak",
            Address = new Address
            {
                AddressId = 1,
                Street = "Testowa",
                Number = "1",
                City = "Test",
                PostalCode = "00-000",
                Country = "Polska"
            }
        };

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(userId, cancellationToken))
            .ReturnsAsync(userInDb);

        // Act
        await _sut.Handle(request, cancellationToken);

        // Assert
        _userRepositoryMock.Verify(
            r => r.GetUserByIdAsync(userId, cancellationToken),
            Times.Once);
    }
}