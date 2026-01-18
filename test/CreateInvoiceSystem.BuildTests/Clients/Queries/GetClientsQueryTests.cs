using CreateInvoiceSystem.Modules.Clients.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Clients.Queries;

public class GetClientsQueryTests
{
    private readonly Mock<IClientRepository> _repositoryMock;

    public GetClientsQueryTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
    }

    [Fact]
    public async Task Execute_ShouldReturnList_WhenClientsExist()
    {
        // Arrange
        var clients = new List<Client> { new() { ClientId = 1, Name = "Test" } };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clients);

        var query = new GetClientsQuery(null);

        // Act
        var result = await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull().And.HaveCount(1);
    }
    
    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenListIsNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Client>)null!);

        var query = new GetClientsQuery(null);

        // Act
        Func<Task> act = async () => await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("List of clients is empty.");
    }

    // TEST 2: Przypadek, gdy repozytorium zwraca pustą listę
    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenListIsEmpty()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client>());

        var query = new GetClientsQuery(null);

        // Act
        Func<Task> act = async () => await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("List of clients is empty.");
    }
}