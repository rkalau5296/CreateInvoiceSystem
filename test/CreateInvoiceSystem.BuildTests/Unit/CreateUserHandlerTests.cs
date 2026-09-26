using CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.CreateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using FluentAssertions;
using Moq;
using User = CreateInvoiceSystem.Modules.Users.Domain.Entities.User;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class CreateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly CreateUserHandler _sut;

    public CreateUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _sut = new CreateUserHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenUserDtoIsNull()
    {
        // Arrange
        var request = new CreateUserRequest(null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request.User");

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenAddressDtoIsNull()
    {
        // Arrange
        var userDto = new CreateUserDto(
            Name: "Jan Kowalski",
            CompanyName: "Test Corp",
            Email: "jan.kowalski@example.com",
            Password: "Password123!",
            Nip: "1234567890",
            IsActive: true,
            Address: null!
        );

        var request = new CreateUserRequest(userDto);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request.User.Address");

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAddUserToRepositoryAndReturnResponse_WhenRequestIsValid()
    {
        // Arrange
        var userDto = CreateValidUserDto();
        var request = new CreateUserRequest(userDto);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _sut.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeNull();
        response.Data!.Email.Should().Be(userDto.Email);
        response.Data.Name.Should().Be(userDto.Name);

        _userRepositoryMock.Verify(
            r => r.AddAsync(It.Is<User>(u => u.Email == userDto.Email), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CreateUserDto CreateValidUserDto()
    {
        var addressDto = new CreateAddressDto(
            Street: "Prosta",
            Number: "10",            
            City: "Warszawa",
            PostalCode: "00-001",
            Country: "Poland"
        );

        return new CreateUserDto(
            Name: "Jan Kowalski",
            CompanyName: "Firma Testowa",
            Email: "jan.kowalski@example.com",
            Password: "SecurePassword123!",
            Nip: "5213849120",
            IsActive: true,
            Address: addressDto
        );
    }
}