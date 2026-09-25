using CreateInvoiceSystem.BuildTests.Base;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.UpdateClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class UpdateClientHandlerTests : BaseTest<IClientRepository>
{
    private readonly UpdateClientHandler _sut;

    public UpdateClientHandlerTests()
    {
        _sut = new UpdateClientHandler(RepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateClientAndReturnResponse_WhenDataIsValid()
    {
        // Arrange
        var addressDto = new AddressDto(
            5,
            "Nowa Ulica",
            "2",
            "Gdynia",
            "81-000",
            "Polska");

        var updateDto = new UpdateClientDto(
            1,
            "Zaktualizowany Klient",
            "9876543210",
            addressDto,
            5,
            10,
            "testc@test.com");

        var request = new UpdateClientRequest(updateDto, 1);

        var existingClient = new Client
        {
            ClientId = 1,
            UserId = 10,
            Name = "Stary Klient",
            Nip = "0000000000",
            AddressId = 5,
            Address = new Address
            {
                AddressId = 5,
                Street = "Stara Ulica",
                Number = "1",
                City = "Gdańsk",
                PostalCode = "80-000",
                Country = "Polska"
            }
        };

        RepositoryMock
            .Setup(r => r.GetByIdAsync(1, 10, CancellationToken))
            .ReturnsAsync(existingClient);

        RepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Client>(), CancellationToken))
            .ReturnsAsync(existingClient);

        // Act
        var result = await _sut.Handle(request, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Zaktualizowany Klient");
        result.Data.Address!.Street.Should().Be("Nowa Ulica");

        RepositoryMock.Verify(
            r => r.GetByIdAsync(1, 10, CancellationToken),
            Times.Once);

        RepositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Client>(client =>
                    client.ClientId == 1
                    && client.UserId == 10
                    && client.Name == "Zaktualizowany Klient"
                    && client.Nip == "9876543210"
                    && client.Email == "testc@test.com"
                    && client.Address!.Street == "Nowa Ulica"
                    && client.Address.Number == "2"
                    && client.Address.City == "Gdynia"
                    && client.Address.PostalCode == "81-000"
                    && client.Address.Country == "Polska"),
                CancellationToken),
            Times.Once);

        RepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ShouldCreateNewAddress_WhenExistingClientHasNoAddress()
    {
        // Arrange
        var addressDto = new AddressDto(0, "Nowa Ulica", "1", "Warszawa", "00-001", "Polska");
        var updateDto = new UpdateClientDto(1, "Klient", "123", addressDto, 0, 1, "testc@test.com");
        var request = new UpdateClientRequest(updateDto, 1);

        var existingClient = new Client { ClientId = 1, UserId = 1, Name = "Klient", Address = null! };

        RepositoryMock
            .Setup(r => r.GetByIdAsync(1, 1, CancellationToken))
            .ReturnsAsync(existingClient);

        RepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Client>(), CancellationToken))
            .ReturnsAsync(existingClient);

        // Act
        await _sut.Handle(request, CancellationToken);

        // Assert
        existingClient.Address.Should().NotBeNull();
        existingClient.Address!.Street.Should().Be("Nowa Ulica");
        existingClient.Address.City.Should().Be("Warszawa");
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenClientNotFound()
    {
        // Arrange
        var updateDto = new UpdateClientDto(99, "Nieistniejący", "123", null!, 0, 1, "testc@test.com");
        var request = new UpdateClientRequest(updateDto, 99);

        RepositoryMock
            .Setup(r => r.GetByIdAsync(99, 1, CancellationToken))
            .ReturnsAsync((Client)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Client with ID 99 not found.");
    }    

    [Fact]
    public void RequestConstructor_ShouldThrowArgumentOutOfRangeException_WhenIdIsLessThanOne()
    {
        // Arrange
        var address = new AddressDto(1, "", "", "", "", "");
        var updateDto = new UpdateClientDto(0, "", "", address, 1, 1, "testc@test.com");

        // Act
        Action act = () => new UpdateClientRequest(updateDto, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Id must be greater than or equal to 1.*");
    }

    [Fact]
    public void RequestConstructor_ShouldThrowArgumentNullException_WhenClientDtoIsNull()
    {
        // Act
        Action act = () => new UpdateClientRequest(null!, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*cannot be null*");
    }
}