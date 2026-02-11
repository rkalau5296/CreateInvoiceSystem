using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Clients.Domain.Entities;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Clients.Queries;

public class GetClientsQueryTests
{
    private readonly Mock<IClientRepository> _repositoryMock;

    public GetClientsQueryTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
    }

    [Fact]
    public async Task Execute_ShouldReturnPagedResult_WhenClientsExist()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var clients = new List<Client> { new() { ClientId = 1, Name = "Test" } };
        var pagedResult = new PagedResult<Client>(clients, 1, pageNumber, pageSize);
        
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), pageNumber, pageSize, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var query = new GetClientsQuery(null, pageNumber, pageSize, null);

        // Act
        var result = await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenResultIsNull()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;        
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), pageNumber, pageSize, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Client>)null!);

        var query = new GetClientsQuery(null, pageNumber, pageSize, null);

        // Act
        Func<Task> act = async () => await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("List of clients is empty.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenRepositoryReturnsNull()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), pageNumber, pageSize, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Client>)null!);

        var query = new GetClientsQuery(null, pageNumber, pageSize, null);

        // Act
        Func<Task> act = async () => await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("List of clients is empty.");
    }
}