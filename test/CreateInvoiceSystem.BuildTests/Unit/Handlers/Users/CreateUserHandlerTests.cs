using CreateInvoiceSystem.Modules.Users.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.CreateUser;
using CreateInvoiceSystem.Modules.Users.Dto;
using CreateInvoiceSystem.Modules.Users.Interfaces;
using FluentAssertions;
using Moq;
using User = CreateInvoiceSystem.Modules.Users.Entities.User;

namespace CreateInvoiceSystem.BuildTests.Unit.Handlers.Users;

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