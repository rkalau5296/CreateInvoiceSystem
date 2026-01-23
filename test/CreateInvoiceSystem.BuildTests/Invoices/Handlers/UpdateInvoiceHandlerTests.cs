using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.UpdateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Invoices.Handlers;

public class UpdateInvoiceHandlerTests
{
    private readonly Mock<ICommandExecutor> _commandExecutorMock;
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly UpdateInvoiceHandler _handler;

    public UpdateInvoiceHandlerTests()
    {
        _commandExecutorMock = new Mock<ICommandExecutor>();
        _repositoryMock = new Mock<IInvoiceRepository>();
        _handler = new UpdateInvoiceHandler(_commandExecutorMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUpdatedInvoiceDto_WhenCommandExecutesSuccessfully()
    {
        // Arrange
        var invoiceId = 55;
        var userId = 1;

        // Mapowanie 20 parametrów UpdateInvoiceDto
        var updateDto = new UpdateInvoiceDto(
            invoiceId, "Title", 100m, 23m, 123m, DateTime.Now, DateTime.Now, "Comments",
            null, userId, null!, "Card", new List<UpdateInvoicePositionDto>(),
            "SellerName", "SellerNip", "SellerAddr", "BankAcc",
            "ClientName", "ClientNip", "ClientAddr"
        );

        var request = new UpdateInvoiceRequest(invoiceId, updateDto);

        _commandExecutorMock
            .Setup(x => x.Execute<UpdateInvoiceDto, UpdateInvoiceDto, IInvoiceRepository>(
                It.IsAny<CommandBase<UpdateInvoiceDto, UpdateInvoiceDto, IInvoiceRepository>>(),
                It.IsAny<IInvoiceRepository>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updateDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.InvoiceId.Should().Be(invoiceId);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenExecutorFails()
    {
        // Arrange
        var invoiceId = 1;
        var updateDto = new UpdateInvoiceDto(
            invoiceId, "Title", 100m, 23m, 123m, DateTime.Now, DateTime.Now, "Comments",
            null, 1, null!, "Card", new List<UpdateInvoicePositionDto>(),
            "S", "SN", "SA", "SB", "CN", "CNIP", "CADDR"
        );

        var request = new UpdateInvoiceRequest(invoiceId, updateDto);

        _commandExecutorMock
             .Setup(x => x.Execute<UpdateInvoiceDto, UpdateInvoiceDto, IInvoiceRepository>(
                 It.IsAny<UpdateInvoiceCommand>(),
                 It.IsAny<IInvoiceRepository>(),
                 It.IsAny<CancellationToken>()))
             .ThrowsAsync(new InvalidOperationException("Concurrency error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Concurrency error");
    }
}