using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.CQRS;
using FluentAssertions;
using Moq;
using Xunit;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.CreateInvoice;

namespace CreateInvoiceSystem.BuildTests.Invoices.Handlers;

public class CreateInvoiceHandlerTests
{
    private readonly Mock<ICommandExecutor> _commandExecutorMock;
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly Mock<IInvoiceEmailSender> _emailSenderMock;
    private readonly CreateInvoiceHandler _handler;

    public CreateInvoiceHandlerTests()
    {
        _commandExecutorMock = new Mock<ICommandExecutor>();
        _repositoryMock = new Mock<IInvoiceRepository>();
        _emailSenderMock = new Mock<IInvoiceEmailSender>();

        _handler = new CreateInvoiceHandler(
            _commandExecutorMock.Object,
            _repositoryMock.Object,
            _emailSenderMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvoiceDto_WhenCommandExecutesSuccessfully()
    {
        // Arrange
        var inputDto = new CreateInvoiceDto
        {
            Title = "FV/2024/01",
            TotalAmount = 100m,
            UserId = 1
        };
        var request = new CreateInvoiceRequest(inputDto);
       
        var expectedInvoice = new InvoiceDto(
            InvoiceId: 500,
            Title: "FV/2024/01",
            TotalAmount: 100m,
            PaymentDate: DateTime.Now.AddDays(7),
            CreatedDate: DateTime.Now,
            Comments: "Test",
            ClientId: 1,
            UserId: 1,
            MethodOfPayment: "Transfer",
            InvoicePositions: new List<InvoicePositionDto>(),
            ClientName: "Client Name",
            ClientNip: "1234567890",
            ClientAddress: "Address"
        );
       
        _commandExecutorMock
            .Setup(x => x.Execute<CreateInvoiceDto, InvoiceDto, IInvoiceRepository>(
                It.IsAny<CommandBase<CreateInvoiceDto, InvoiceDto, IInvoiceRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInvoice);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.InvoiceId.Should().Be(500);
        result.Data.Title.Should().Be("FV/2024/01");

        _commandExecutorMock.Verify(x => x.Execute<CreateInvoiceDto, InvoiceDto, IInvoiceRepository>(
            It.IsAny<CommandBase<CreateInvoiceDto, InvoiceDto, IInvoiceRepository>>(),
            _repositoryMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenExecutorFails()
    {
        // Arrange
        var request = new CreateInvoiceRequest(new CreateInvoiceDto());
       
        _commandExecutorMock
            .Setup(x => x.Execute<CreateInvoiceDto, InvoiceDto, IInvoiceRepository>(
                It.IsAny<CommandBase<CreateInvoiceDto, InvoiceDto, IInvoiceRepository>>(),
                _repositoryMock.Object,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Email service down"));

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email service down");
    }
}