using CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Clients.Commands;

public class UpdateClientCommandTests
{
    private readonly Mock<IClientRepository> _repositoryMock;

    public UpdateClientCommandTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
    }

    [Fact]
    public async Task Execute_ShouldUpdateClientAndReturnDto_WhenDataIsValid()
    {
        // Arrange
        var addressDto = new AddressDto(5, "Nowa Ulica", "2", "Gdynia", "81-000", "Polska");
        var updateDto = new UpdateClientDto(1, "Zaktualizowany Klient", "9876543210", addressDto, 5, 10);

        var existingClient = new Client
        {
            ClientId = 1,
            UserId = 10,
            Name = "Stary Klient",
            Nip = "0000000000",
            AddressId = 5,
            Address = new Address { AddressId = 5, Street = "Stara Ulica", Number = "1", City = "Gdańsk", PostalCode = "80-000", Country = "Polska" }
        };

        var command = new UpdateClientCommand { Parametr = updateDto };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        // Act
        var result = await command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Zaktualizowany Klient");
        result.Address.Street.Should().Be("Nowa Ulica");

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowArgumentNullException_WhenParametrIsNull()
    {
        // Arrange
        var command = new UpdateClientCommand { Parametr = null! };

        // Act
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("Parametr");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenClientNotFound()
    {
        // Arrange
        var updateDto = new UpdateClientDto(99, "Nieistniejący", "123", null!, 0, 1);
        var command = new UpdateClientCommand { Parametr = updateDto };

        _repositoryMock.Setup(r => r.GetByIdAsync(99, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client)null!);

        // Act
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Client with ID 99 not found.");
    }

    [Fact]
    public async Task Execute_ShouldCreateNewAddress_WhenExistingClientHasNoAddress()
    {
        // Arrange
        var addressDto = new AddressDto(0, "Nowa Ulica", "1", "Warszawa", "00-001", "Polska");
        var updateDto = new UpdateClientDto(1, "Klient", "123", addressDto, 0, 1);

        var existingClient = new Client { ClientId = 1, UserId = 1, Name = "Klient", Address = null };
        var command = new UpdateClientCommand { Parametr = updateDto };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        // Act
        await command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        existingClient.Address.Should().NotBeNull();
        existingClient.Address!.Street.Should().Be("Nowa Ulica");
    }
}