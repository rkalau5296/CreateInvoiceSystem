using Moq;
using FluentAssertions;
using Xunit;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;

namespace CreateInvoiceSystem.BuildTests.Invoices.Queries;

public class GetInvoicesQueryTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;

    public GetInvoicesQueryTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
    }

    [Fact]
    public async Task Execute_ShouldReturnListOfInvoices_WhenInvoicesExist()
    {
        // Arrange
        int? userId = 1;
        var query = new GetInvoicesQuery(userId);
        var invoices = new List<Invoice>
        {
            new Invoice { InvoiceId = 1, UserId = 1, Title = "Invoice 1" },
            new Invoice { InvoiceId = 2, UserId = 1, Title = "Invoice 2" }
        };

        _repositoryMock.Setup(r => r.GetInvoicesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoices);

        // Act
        var result = await query.Execute(_repositoryMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(invoices);
        _repositoryMock.Verify(r => r.GetInvoicesAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenRepositoryReturnsNull()
    {
        // Arrange
        int? userId = 1;
        var query = new GetInvoicesQuery(userId);

        _repositoryMock.Setup(r => r.GetInvoicesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Invoice>)null);

        // Act
        Func<Task> act = async () => await query.Execute(_repositoryMock.Object);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("List of addresses is empty.");
    }

    [Fact]
    public async Task Execute_ShouldReturnEmptyList_WhenNoInvoicesFoundButRepositoryReturnsEmptyList()
    {
        // Arrange
        int? userId = 99;
        var query = new GetInvoicesQuery(userId);
        var emptyList = new List<Invoice>();

        _repositoryMock.Setup(r => r.GetInvoicesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await query.Execute(_repositoryMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}