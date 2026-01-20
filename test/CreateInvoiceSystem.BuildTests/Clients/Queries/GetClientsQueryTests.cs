using CreateInvoiceSystem.Abstractions;
using CreateInvoiceSystem.Abstractions.Pagination;
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
    public async Task Execute_ShouldReturnPagedResult_WhenClientsExist()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var clients = new List<Client> { new() { ClientId = 1, Name = "Test" } };
        var pagedResult = new PagedResult<Client>(clients, 1, pageNumber, pageSize);

        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var query = new GetClientsQuery(null, pageNumber, pageSize);

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
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Client>)null!);

        var query = new GetClientsQuery(null, pageNumber, pageSize);

        // Act
        Func<Task> act = async () => await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("List of clients is empty.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenTotalCountIsZero()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var emptyPagedResult = new PagedResult<Client>(new List<Client>(), 0, pageNumber, pageSize);

        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedResult);

        var query = new GetClientsQuery(null, pageNumber, pageSize);

        // Act
        Func<Task> act = async () => await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("List of clients is empty.");
    }
}