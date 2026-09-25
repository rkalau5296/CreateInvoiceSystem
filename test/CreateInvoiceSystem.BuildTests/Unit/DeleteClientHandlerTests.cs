using CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.DeleteClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class DeleteClientHandlerTests
{
    private readonly Mock<IClientRepository> _repositoryMock;
    private readonly DeleteClientHandler _sut;

    public DeleteClientHandlerTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
        _sut = new DeleteClientHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnClientDto_WhenDeletionIsSuccessful()
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

        var request = new DeleteClientRequest(1) { UserId = 10 };

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientEntity);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.ClientId.Should().Be(1);

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(1, 10, It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.RemoveAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.RemoveAddressAsync(5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenClientNotFound()
    {
        // Arrange
        var request = new DeleteClientRequest(99) { UserId = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(99, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Client with ID 99 not found.");
    }

    [Fact]
    public async Task Handle_ShouldNotTryToRemoveAddress_WhenAddressIsNull_InEntity()
    {
        // Arrange
        var clientEntity = new Client
        {
            ClientId = 1,
            UserId = 1,
            AddressId = 0,
            Address = null!
        };

        var request = new DeleteClientRequest(1) { UserId = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientEntity);

        // Act & Assert
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("address");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenIdIsLessThanOne()
    {
        // Arrange
        int invalidId = 0;

        // Act
        Action act = () => new DeleteClientRequest(invalidId);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("id");
    }
}