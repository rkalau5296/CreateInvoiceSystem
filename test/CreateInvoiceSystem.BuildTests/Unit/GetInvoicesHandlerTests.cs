using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoices;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class GetInvoicesHandlerTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly GetInvoicesHandler _sut;

    public GetInvoicesHandlerTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
        _sut = new GetInvoicesHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvoicesList_WhenInvoicesExist()
    {
        // Arrange
        const int userId = 10;
        const int pageNumber = 1;
        const int pageSize = 10;
        const string searchTerm = "FV";

        var request = new GetInvoicesRequest
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var invoiceList = new List<Invoice>
        {
            new() { InvoiceId = 1, Title = "FV 1", UserId = userId },
            new() { InvoiceId = 2, Title = "FV 2", UserId = userId }
        };

        var pagedResult = new PagedResult<Invoice>(invoiceList, 2, pageNumber, pageSize);

        _repositoryMock
            .Setup(r => r.GetInvoicesAsync(userId, pageNumber, pageSize, searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);

        _repositoryMock.Verify(
            r => r.GetInvoicesAsync(userId, pageNumber, pageSize, searchTerm, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoInvoicesFound()
    {
        // Arrange
        const int userId = 99;
        const int pageNumber = 1;
        const int pageSize = 10;
        const string searchTerm = "NonExistent";

        var request = new GetInvoicesRequest
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var emptyPagedResult = new PagedResult<Invoice>(new List<Invoice>(), 0, pageNumber, pageSize);

        _repositoryMock
            .Setup(r => r.GetInvoicesAsync(userId, pageNumber, pageSize, searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedResult);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();

        _repositoryMock.Verify(
            r => r.GetInvoicesAsync(userId, pageNumber, pageSize, searchTerm, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenRepositoryReturnsNull()
    {
        // Arrange
        const int userId = 1;
        var request = new GetInvoicesRequest { UserId = userId, PageNumber = 1, PageSize = 10 };

        _repositoryMock
            .Setup(r => r.GetInvoicesAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<Invoice>)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        _repositoryMock.Verify(
            r => r.GetInvoicesAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryFails()
    {
        // Arrange
        var request = new GetInvoicesRequest { PageNumber = 1, PageSize = 10 };

        _repositoryMock
            .Setup(r => r.GetInvoicesAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection error"));

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection error");
    }
}