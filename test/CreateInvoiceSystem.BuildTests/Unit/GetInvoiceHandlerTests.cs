using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class GetInvoiceHandlerTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly GetInvoiceHandler _sut;

    public GetInvoiceHandlerTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
        _sut = new GetInvoiceHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvoiceDto_WhenInvoiceExists()
    {
        // Arrange
        const int invoiceId = 10;
        const int userId = 1;
        var request = new GetInvoiceRequest(userId, invoiceId);

        var expectedInvoice = new Invoice
        {
            InvoiceId = invoiceId,
            UserId = userId,
            Title = "FV/2026/01",
            InvoicePositions = new List<InvoicePosition>()
        };

        _repositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInvoice);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.InvoiceId.Should().Be(invoiceId);

        _repositoryMock.Verify(
            r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenInvoiceDoesNotExist()
    {
        // Arrange
        const int userId = 1;
        const int invoiceId = 999;
        var request = new GetInvoiceRequest(userId, invoiceId);

        _repositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Invoice with ID {invoiceId} not found.");

        _repositoryMock.Verify(
            r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()),
            Times.Once);
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