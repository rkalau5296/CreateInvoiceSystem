using CreateInvoiceSystem.Modules.Clients.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Clients.Queries;

public class GetClientQueryTests
{
    private readonly Mock<IClientRepository> _repositoryMock;

    public GetClientQueryTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
    }

    [Fact]
    public async Task Execute_ShouldReturnClient_WhenClientExists()
    {
        // Arrange
        var clientId = 1;
        var userId = 10;
        var expectedClient = new Client
        {
            ClientId = clientId,
            Name = "Test Client",
            UserId = userId
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(clientId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedClient);

        var query = new GetClientQuery(clientId, userId);

        // Act
        var result = await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(clientId);
        result.Name.Should().Be("Test Client");

        _repositoryMock.Verify(r => r.GetByIdAsync(clientId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = 99;
        int? userId = null;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(clientId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client)null!);

        var query = new GetClientQuery(clientId, userId);

        // Act
        Func<Task> act = async () => await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Client with ID {clientId} not found or access denied.");
    }

    [Fact]
    public async Task Execute_ShouldPassCorrectParametersToRepository()
    {
        // Arrange
        var clientId = 5;
        var userId = 100;
        var query = new GetClientQuery(clientId, userId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(clientId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client { ClientId = clientId });

        // Act
        await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetByIdAsync(
            It.Is<int>(id => id == clientId),
            It.Is<int?>(u => u == userId),
            It.IsAny<CancellationToken>()),
        Times.Once);
    }
}