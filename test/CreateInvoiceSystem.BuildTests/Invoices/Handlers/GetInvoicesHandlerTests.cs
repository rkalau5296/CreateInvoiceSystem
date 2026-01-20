using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.Pagination;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoices;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using FluentAssertions;
using Moq;


namespace CreateInvoiceSystem.BuildTests.Invoices.Handlers;

public class GetInvoicesHandlerTests
{
    private readonly Mock<IQueryExecutor> _queryExecutorMock;
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly GetInvoicesHandler _handler;

    public GetInvoicesHandlerTests()
    {
        _queryExecutorMock = new Mock<IQueryExecutor>();
        _repositoryMock = new Mock<IInvoiceRepository>();
        _handler = new GetInvoicesHandler(_queryExecutorMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvoicesList_WhenQueryReturnsData()
    {
        // Arrange
        var userId = 10;
        var request = new GetInvoicesRequest { UserId = userId, PageNumber = 1, PageSize = 10 };

        var invoiceList = new List<Invoice>
    {
        new() { InvoiceId = 1, Title = "FV 1", UserId = userId },
        new() { InvoiceId = 2, Title = "FV 2", UserId = userId }
    };
        
        var pagedResult = new PagedResult<Invoice>(invoiceList, 2, 1, 10);

        _queryExecutorMock
            .Setup(x => x.Execute<PagedResult<Invoice>, IInvoiceRepository>(
                It.IsAny<GetInvoicesQuery>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);

        _queryExecutorMock.Verify(x => x.Execute<PagedResult<Invoice>, IInvoiceRepository>(
            It.IsAny<GetInvoicesQuery>(),
            _repositoryMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoInvoicesExist()
    {
        // Arrange
        var request = new GetInvoicesRequest { UserId = 1, PageNumber = 1, PageSize = 10 };
        
        var emptyPagedResult = new PagedResult<Invoice>(new List<Invoice>(), 0, 1, 10);

        _queryExecutorMock
            .Setup(x => x.Execute<PagedResult<Invoice>, IInvoiceRepository>(
                It.IsAny<GetInvoicesQuery>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedResult);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty(); 
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenExecutorFails()
    {
        // Arrange
        var request = new GetInvoicesRequest { PageNumber = 1, PageSize = 10 };
        
        _queryExecutorMock
            .Setup(x => x.Execute<PagedResult<Invoice>, IInvoiceRepository>(
                It.IsAny<GetInvoicesQuery>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection error");
    }
}