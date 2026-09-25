using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.DeleteInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class DeleteInvoiceHandlerTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly DeleteInvoiceHandler _sut;

    public DeleteInvoiceHandlerTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
        _sut = new DeleteInvoiceHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenInvoiceIsSuccessfullyDeleted()
    {
        // Arrange
        const int invoiceId = 10;
        const int userId = 1;
        var request = new DeleteInvoiceRequest(invoiceId) { UserId = userId };

        var invoice = new Invoice
        {
            InvoiceId = invoiceId,
            UserId = userId,
            Title = "Deleted Invoice",
            InvoicePositions = new List<InvoicePosition>
            {
                new()
                {
                    InvoicePositionId = 1,
                    InvoiceId = invoiceId
                }
            }
        };

        _repositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(
                userId,
                invoiceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.InvoiceId.Should().Be(invoiceId);

        _repositoryMock.Verify(
            r => r.GetInvoiceByIdAsync(
                userId,
                invoiceId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            r => r.RemoveRangeAsync(
                invoice.InvoicePositions,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            r => r.RemoveAsync(
                invoice,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenInvoiceNotFound()
    {
        // Arrange
        var request = new DeleteInvoiceRequest(99) { UserId = 1 };

        _repositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invoice with ID 99 not found.");
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenInvoiceDeletionFails()
    {
        // Arrange
        const int invoiceId = 10;
        const int userId = 1;
        var request = new DeleteInvoiceRequest(invoiceId) { UserId = userId };

        var invoice = new Invoice
        {
            InvoiceId = invoiceId,
            UserId = userId
        };

        _repositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(
                userId,
                invoiceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _repositoryMock
            .Setup(r => r.RemoveAsync(
                invoice,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invoice deletion failed."));

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invoice deletion failed.");

        _repositoryMock.Verify(
            r => r.GetInvoiceByIdAsync(
                userId,
                invoiceId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            r => r.RemoveAsync(
                invoice,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            r => r.RemoveRangeAsync(
                It.IsAny<IEnumerable<InvoicePosition>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ShouldNotCallRemoveRange_WhenInvoiceHasNoPositions()
    {
        // Arrange
        const int invoiceId = 10;
        const int userId = 1;
        var request = new DeleteInvoiceRequest(invoiceId) { UserId = userId };

        var invoiceEntity = new Invoice
        {
            InvoiceId = invoiceId,
            UserId = userId,
            InvoicePositions = new List<InvoicePosition>()
        };

        _repositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity);

        _repositoryMock
            .Setup(r => r.InvoiceExistsAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.InvoicePositionExistsAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.RemoveRangeAsync(
                It.IsAny<IEnumerable<InvoicePosition>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            r => r.RemoveAsync(
                invoiceEntity,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}