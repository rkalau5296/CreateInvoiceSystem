using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.UpdateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

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
        var updateDto = new UpdateInvoiceDto(invoiceId, "Title", 100m, DateTime.Now, DateTime.Now, "", null, 1, null, "Card", new List<UpdateInvoicePositionDto>(), "", "", "");
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
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenExecutorFails()
    {
        // Arrange
        var updateDto = new UpdateInvoiceDto(1, "Title", 100m, DateTime.Now, DateTime.Now, "", null, 1, null, "Card", new List<UpdateInvoicePositionDto>(), "", "", "");
        var request = new UpdateInvoiceRequest(1, updateDto);
        
        _commandExecutorMock
             .Setup(x => x.Execute(
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