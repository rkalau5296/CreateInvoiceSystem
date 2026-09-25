using CreateInvoiceSystem.BuildTests.Base;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class GetClientHandlerTests : BaseTest<IClientRepository>
{
    private readonly GetClientHandler _sut;

    public GetClientHandlerTests()
    {
        _sut = new GetClientHandler(RepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnClientDto_WhenClientExists()
    {
        // Arrange
        var clientId = 1;
        var userId = 100;
        var request = new GetClientRequest(clientId) { UserId = userId };

        var clientEntity = new Client
        {
            ClientId = clientId,
            UserId = userId,
            Name = "Test Client",
            Address = new Address
            {
                Street = "Street",
                City = "City",
                Number = "1",
                PostalCode = "00-000",
                Country = "Poland"
            }
        };

        RepositoryMock
            .Setup(r => r.GetByIdAsync(clientId, userId, CancellationToken))
            .ReturnsAsync(clientEntity);

        // Act
        var result = await _sut.Handle(request, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data!.ClientId.Should().Be(clientId);
        result.Data.Name.Should().Be("Test Client");

        RepositoryMock.Verify(
            r => r.GetByIdAsync(clientId, userId, CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = 99;
        int? userId = null;
        var request = new GetClientRequest(clientId) { UserId = userId };

        RepositoryMock
            .Setup(r => r.GetByIdAsync(clientId, userId, CancellationToken))
            .ReturnsAsync((Client)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Client with ID {clientId} not found or access denied.");
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectParametersToRepository()
    {
        // Arrange
        var clientId = 5;
        var userId = 100;
        var request = new GetClientRequest(clientId) { UserId = userId };

        var clientEntity = new Client
        {
            ClientId = clientId,
            UserId = userId,
            Name = "Client 5",
            Address = new Address
            {
                Street = "Street",
                City = "City",
                Number = "1",
                PostalCode = "00-000",
                Country = "Poland"
            }
        };

        RepositoryMock
            .Setup(r => r.GetByIdAsync(clientId, userId, CancellationToken))
            .ReturnsAsync(clientEntity);

        // Act
        await _sut.Handle(request, CancellationToken);

        // Assert
        RepositoryMock.Verify(
            r => r.GetByIdAsync(
                It.Is<int>(id => id == clientId),
                It.Is<int?>(u => u == userId),
                CancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        var request = new GetClientRequest(1) { UserId = 1 };
        var errorMessage = "Database error";

        RepositoryMock
            .Setup(r => r.GetByIdAsync(1, 1, CancellationToken))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage(errorMessage);
    }

    [Fact]
    public void RequestConstructor_ShouldThrowArgumentOutOfRangeException_WhenIdIsLessThanOne()
    {
        // Act
        Action act = () => new GetClientRequest(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Id must be greater than or equal to 1.*");
    }
}