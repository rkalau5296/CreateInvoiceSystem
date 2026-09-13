using CreateInvoiceSystem.Modules.Clients.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

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
            Address = new Address
            {
                AddressId = 5,
                Street = "Testowa"
            }
        };

        var command = new DeleteClientCommand
        {
            Parametr = clientEntity
        };

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientEntity);

        // Act
        var result = await command.Execute(
            _repositoryMock.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(1);

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.RemoveAsync(
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.RemoveAddressAsync(
                5,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.VerifyNoOtherCalls();
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