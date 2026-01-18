using CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Domain.Dto;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Domain.Mappers;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Clients.Commands;

public class DeleteClientCommandTests
{
    private readonly Mock<IClientRepository> _repositoryMock;

    public DeleteClientCommandTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
    }

    [Fact]
    public async Task Execute_ShouldReturnDto_WhenDeletionIsSuccessful()
    {
        // Arrange
        var clientEntity = new Client
        {
            ClientId = 1,
            UserId = 10,
            AddressId = 5,
            Address = new Address { AddressId = 5, Street = "Testowa" }
        };

        var command = new DeleteClientCommand { Parametr = clientEntity };

        
        _repositoryMock.Setup(r => r.GetByIdAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientEntity);
        
        _repositoryMock.Setup(r => r.ExistsByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        _repositoryMock.Setup(r => r.AddressExistsByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(1);

        _repositoryMock.Verify(r => r.RemoveAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.RemoveAddressAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenClientNotFound()
    {
        // Arrange
        var command = new DeleteClientCommand { Parametr = new Client { ClientId = 99, UserId = 1 } };

        _repositoryMock.Setup(r => r.GetByIdAsync(99, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client)null!);

        // Act
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Client with ID 99 not found.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenDeletionFails()
    {
        // Arrange
        var clientEntity = new Client
        {
            ClientId = 1,
            UserId = 1,
            AddressId = 5,            
            Address = new Address { AddressId = 5, Street = "Test", Number = "1", City = "Test", PostalCode = "00-000", Country = "PL" }
        };

        var command = new DeleteClientCommand { Parametr = clientEntity };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientEntity);
        
        _repositoryMock.Setup(r => r.ExistsByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to delete Client or Client address with ID 1.");
    }

    [Fact]
    public async Task Execute_ShouldNotTryToRemoveAddress_WhenAddressIsNull_InEntity()
    {
        // Arrange        
        var clientEntity = new Client
        {
            ClientId = 1,
            UserId = 1,
            AddressId = 0,
            Address = null 
        };

        var command = new DeleteClientCommand { Parametr = clientEntity };
        
        _repositoryMock.Setup(r => r.GetByIdAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientEntity);

        _repositoryMock.Setup(r => r.ExistsByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        // Act & Assert
        
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object, CancellationToken.None);
        
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("address");
    }
}