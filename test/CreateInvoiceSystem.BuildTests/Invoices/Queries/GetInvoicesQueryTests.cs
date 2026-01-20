using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Invoices.Queries;

public class GetInvoicesQueryTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;

    public GetInvoicesQueryTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
    }

    [Fact]
    public async Task Execute_ShouldReturnPagedResultOfInvoices_WhenInvoicesExist()
    {
        // Arrange
        int? userId = 1;
        int pageNumber = 1;
        int pageSize = 10;
        var query = new GetInvoicesQuery(userId, pageNumber, pageSize);

        var invoices = new List<Invoice>
        {
            new Invoice { InvoiceId = 1, UserId = 1, Title = "Invoice 1" },
            new Invoice { InvoiceId = 2, UserId = 1, Title = "Invoice 2" }
        };
        var pagedResult = new PagedResult<Invoice>(invoices, 2, pageNumber, pageSize);

        _repositoryMock.Setup(r => r.GetInvoicesAsync(userId, pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await query.Execute(_repositoryMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(invoices);
        result.TotalCount.Should().Be(2);
        _repositoryMock.Verify(r => r.GetInvoicesAsync(userId, pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenRepositoryReturnsNull()
    {
        // Arrange
        int? userId = 1;
        var query = new GetInvoicesQuery(userId, 1, 10);

        _repositoryMock.Setup(r => r.GetInvoicesAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Invoice>)null);

        // Act
        Func<Task> act = async () => await query.Execute(_repositoryMock.Object);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Execute_ShouldReturnEmptyPagedResult_WhenNoInvoicesFound()
    {
        // Arrange
        int? userId = 99;
        int pageNumber = 1;
        int pageSize = 10;
        var query = new GetInvoicesQuery(userId, pageNumber, pageSize);
        var emptyPagedResult = new PagedResult<Invoice>(new List<Invoice>(), 0, pageNumber, pageSize);

        _repositoryMock.Setup(r => r.GetInvoicesAsync(userId, pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedResult);

        // Act
        var result = await query.Execute(_repositoryMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}