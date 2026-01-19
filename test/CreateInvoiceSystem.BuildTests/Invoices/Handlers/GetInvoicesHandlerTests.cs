using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoices;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

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
        var request = new GetInvoicesRequest { UserId = userId };

        var invoiceList = new List<Invoice>
        {
            new() { InvoiceId = 1, Title = "FV 1", UserId = userId },
            new() { InvoiceId = 2, Title = "FV 2", UserId = userId }
        };
        
        _queryExecutorMock
            .Setup(x => x.Execute<List<Invoice>, IInvoiceRepository>(
                It.IsAny<QueryBase<List<Invoice>, IInvoiceRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceList);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);

        _queryExecutorMock.Verify(x => x.Execute<List<Invoice>, IInvoiceRepository>(
            It.IsAny<GetInvoicesQuery>(),
            _repositoryMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoInvoicesExist()
    {
        // Arrange
        var request = new GetInvoicesRequest { UserId = 1 };
        _queryExecutorMock
            .Setup(x => x.Execute<List<Invoice>, IInvoiceRepository>(
                It.IsAny<QueryBase<List<Invoice>, IInvoiceRepository>>(),
                It.IsAny<IInvoiceRepository>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Invoice>());

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenExecutorFails()
    {
        // Arrange
        var request = new GetInvoicesRequest();
        _queryExecutorMock
            .Setup(x => x.Execute<List<Invoice>, IInvoiceRepository>(
                It.IsAny<QueryBase<List<Invoice>, IInvoiceRepository>>(),
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