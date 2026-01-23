using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.DeleteInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Invoices.Handlers;

public class DeleteInvoiceHandlerTests
{
    private readonly Mock<ICommandExecutor> _commandExecutorMock;
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly DeleteInvoiceHandler _handler;

    public DeleteInvoiceHandlerTests()
    {
        _commandExecutorMock = new Mock<ICommandExecutor>();
        _repositoryMock = new Mock<IInvoiceRepository>();
        _handler = new DeleteInvoiceHandler(_commandExecutorMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvoiceDto_WhenCommandExecutesSuccessfully()
    {
        // Arrange
        var invoiceId = 123;
        var userId = 1;
        var request = new DeleteInvoiceRequest(invoiceId) { UserId = userId };

        // Zakładamy strukturę InvoiceDto zbliżoną do Update (łącznie ok 18-20 parametrów)
        var returnDto = new InvoiceDto(
            invoiceId,
            "Deleted Invoice",
            100m,       // TotalNet
            23m,        // TotalVat
            123m,       // TotalGross
            DateTime.Now,
            DateTime.Now,
            "Comment",
            null,       // ClientId
            userId,
            "Transfer", // MethodOfPayment
            null,       // Client (Dto)
            "SellerName",
            "SellerNip",
            "SellerAddress",
            "BankAccount",
            "ClientName",
            "ClientNip",
            "ClientAddress"
        );

        _commandExecutorMock
            .Setup(x => x.Execute<Invoice, InvoiceDto, IInvoiceRepository>(
                It.IsAny<CommandBase<Invoice, InvoiceDto, IInvoiceRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(returnDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.InvoiceId.Should().Be(invoiceId);

        _commandExecutorMock.Verify(x => x.Execute<Invoice, InvoiceDto, IInvoiceRepository>(
            It.Is<DeleteInvoiceCommand>(c =>
                c.Parametr.InvoiceId == invoiceId &&
                c.Parametr.UserId == userId),
            _repositoryMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenExecutorFails()
    {
        // Arrange
        var request = new DeleteInvoiceRequest(1) { UserId = 1 };

        _commandExecutorMock
            .Setup(x => x.Execute<Invoice, InvoiceDto, IInvoiceRepository>(
                It.IsAny<CommandBase<Invoice, InvoiceDto, IInvoiceRepository>>(),
                It.IsAny<IInvoiceRepository>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Delete failed"));

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Delete failed");
    }
}