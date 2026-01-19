using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Invoices.Handlers;

public class GetInvoiceHandlerTests
{
    private readonly Mock<IQueryExecutor> _queryExecutorMock;
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly GetInvoiceHandler _handler;

    public GetInvoiceHandlerTests()
    {
        _queryExecutorMock = new Mock<IQueryExecutor>();
        _repositoryMock = new Mock<IInvoiceRepository>();
        _handler = new GetInvoiceHandler(_queryExecutorMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvoiceDto_WhenQueryReturnsInvoice()
    {
        // Arrange
        var invoiceId = 1;
        var userId = 100;
        var request = new GetInvoiceRequest(userId, invoiceId);

        var invoiceEntity = new Invoice
        {
            InvoiceId = invoiceId,
            UserId = userId,
            Title = "FV/2026/01"
        };

        _queryExecutorMock
            .Setup(x => x.Execute<Invoice, IInvoiceRepository>(
                It.IsAny<QueryBase<Invoice, IInvoiceRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.InvoiceId.Should().Be(invoiceId);        
        _queryExecutorMock.Verify(x => x.Execute<Invoice, IInvoiceRepository>(
            It.IsAny<GetInvoiceQuery>(),
            _repositoryMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenQueryExecutorFails()
    {
        // Arrange
        var request = new GetInvoiceRequest(1);

        _queryExecutorMock
            .Setup(x => x.Execute<Invoice, IInvoiceRepository>(
                It.IsAny<QueryBase<Invoice, IInvoiceRepository>>(),
                It.IsAny<IInvoiceRepository>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Not found"));

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void Request_ShouldThrowException_WhenIdIsLessThanOne()
    {
        // Act
        Action act = () => new GetInvoiceRequest(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("Id");
    }
}