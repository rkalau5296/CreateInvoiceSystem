using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.UpdateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;
using User = CreateInvoiceSystem.Modules.Users.Domain.Entities.User;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Users;

public class UpdateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly UpdateUserHandler _sut;

    public UpdateUserHandlerTests()
    {
        _sut = new UpdateUserHandler(_userRepositoryMock.Object);
    }    

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Act
        Func<Task> act = async () => await _sut.Handle(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenUserDoesNotExist()
    {
        // Arrange
        var dto = CreateTestUserDto(99) with { Name = "Jan Kowalski" };
        var request = new UpdateUserRequest(dto, 99);

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with ID 99 not found.");

        _userRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenAddressProvidedButUserHasNoAddressInDb()
    {
        // Arrange
        var addressDto = CreateTestUpdateAddressDto() with { Street = "Nowa" };
        var dto = CreateTestUserDto(1) with { Address = addressDto };
        var request = new UpdateUserRequest(dto, 1);

        var userInDb = new User
        {
            UserId = 1,
            Name = "Jan Kowalski",
            Address = null!
        };

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userInDb);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Address for user 1 not found.");

        _userRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserAndAddress_AndReturnUpdatedUserDto_WhenDataIsValid()
    {
        // Arrange
        var addressDto = CreateTestUpdateAddressDto() with
        {
            Street = "Kwiatowa",
            Number = "10A",
            City = "Warszawa",
            PostalCode = "00-001",
            Country = "Polska"
        };

        var dto = new UpdateUserDto(
            UserId: 1,
            Name: "Jan",
            CompanyName: "Nowa Firma",
            Email: "jan.nowy@example.com",
            Nip: "1234567890",
            BankAccountNumber: "PL12345678901234567890123456",
            Address: addressDto);

        var request = new UpdateUserRequest(dto, 1);

        var userInDb = new User
        {
            UserId = 1,
            Name = "Stare Imie",
            CompanyName = "Stara Firma",
            Email = "stary@example.com",
            Nip = "0000000000",
            BankAccountNumber = "PL00000000000000000000000000",
            Address = new Address
            {
                AddressId = 5,
                Street = "Stara",
                Number = "1",
                City = "Krakow",
                PostalCode = "30-000",
                Country = "Polska"
            }
        };

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userInDb);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.UserId.Should().Be(1);
        result.Data.Name.Should().Be("Jan");
        result.Data.CompanyName.Should().Be("Nowa Firma");
        result.Data.Email.Should().Be("jan.nowy@example.com");
        result.Data.Address.Should().NotBeNull();
        result.Data.Address!.Street.Should().Be("Kwiatowa");

        _userRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<User>(u =>
                u.UserId == 1 &&
                u.Name == "Jan" &&
                u.CompanyName == "Nowa Firma" &&
                u.Email == "jan.nowy@example.com" &&
                u.Address!.Street == "Kwiatowa" &&
                u.Address.City == "Warszawa"
            ), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldKeepExistingValues_WhenNullablePropertiesInDtoAreNull()
    {
        // Arrange
        var dto = new UpdateUserDto(
            UserId: 1,
            Name: "Nowe Imie",
            CompanyName: null!,
            Email: null!,
            Nip: null!,
            BankAccountNumber: null!,
            Address: null!);

        var request = new UpdateUserRequest(dto, 1);

        var userInDb = new User
        {
            UserId = 1,
            Name = "Stare Imie",
            CompanyName = "Zachowana Firma",
            Email = "zachowany@example.com",
            Nip = "9999999999",
            BankAccountNumber = "PL99999999999999999999999999",
            Address = new Address
            {
                AddressId = 1,
                Street = "Zachowana Ulica"
            }
        };

        _userRepositoryMock
            .Setup(r => r.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userInDb);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Name.Should().Be("Nowe Imie");
        result.Data.CompanyName.Should().Be("Zachowana Firma");
        result.Data.Email.Should().Be("zachowany@example.com");

        _userRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<User>(u =>
                u.Name == "Nowe Imie" &&
                u.CompanyName == "Zachowana Firma" &&
                u.Email == "zachowany@example.com"
            ), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #region Helper Methods

    private static UpdateUserDto CreateTestUserDto(int userId = 1) => new(
        UserId: userId,
        Name: "Test User",
        CompanyName: "Test Company",
        Email: "test@example.com",
        Nip: "1234567890",
        BankAccountNumber: "PL12345678901234567890123456",
        Address: null!);

    private static UpdateAddressDto CreateTestUpdateAddressDto() => new(
        Street: "Test Street",
        Number: "1",
        City: "Test City",
        PostalCode: "00-000",
        Country: "Poland");

    #endregion
}