using Moq;
using FluentAssertions;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class DeleteInvoiceCommandTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;

    public DeleteInvoiceCommandTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
    }

    [Fact]
    public async Task Execute_ShouldReturnDto_WhenInvoiceIsSuccessfullyDeleted()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceId = 10,
            UserId = 1,
            Title = "Deleted Invoice",
            InvoicePositions = new List<InvoicePosition>
        {
            new()
            {
                InvoicePositionId = 1,
                InvoiceId = 10
            }
        }
        };

        var command = new DeleteInvoiceCommand
        {
            Parametr = invoice
        };

        _repositoryMock
            .Setup(repository => repository.GetInvoiceByIdAsync(
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        // Act
        var result = await command.Execute(
            _repositoryMock.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.InvoiceId.Should().Be(10);

        _repositoryMock.Verify(
            repository => repository.GetInvoiceByIdAsync(
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.RemoveRangeAsync(
                invoice.InvoicePositions,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.RemoveAsync(
                invoice,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenInvoiceNotFound()
    {
        // Arrange
        var invoiceParam = new Invoice { InvoiceId = 99, UserId = 1 };
        var command = new DeleteInvoiceCommand { Parametr = invoiceParam };

        _repositoryMock.Setup(r => r.GetInvoiceByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice)null!);

        // Act
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invoice with ID 99 not found.");
    }

    [Fact]
    public async Task Execute_ShouldPropagateException_WhenInvoiceDeletionFails()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceId = 10,
            UserId = 1
        };

        var command = new DeleteInvoiceCommand
        {
            Parametr = invoice
        };

        _repositoryMock
            .Setup(repository => repository.GetInvoiceByIdAsync(
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _repositoryMock
            .Setup(repository => repository.RemoveAsync(
                invoice,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(
                "Invoice deletion failed."));

        // Act
        Func<Task> act = () => command.Execute(
            _repositoryMock.Object,
            CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Invoice deletion failed.");

        _repositoryMock.Verify(
            repository => repository.GetInvoiceByIdAsync(
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.RemoveAsync(
                invoice,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.RemoveRangeAsync(
                It.IsAny<IEnumerable<InvoicePosition>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_ShouldNotCallRemoveRange_WhenInvoiceHasNoPositions()
    {
        // Arrange
        var invoiceParam = new Invoice { InvoiceId = 10, UserId = 1 };
        var command = new DeleteInvoiceCommand { Parametr = invoiceParam };

        var invoiceEntity = new Invoice { InvoiceId = 10, UserId = 1, InvoicePositions = new List<InvoicePosition>() };

        _repositoryMock.Setup(r => r.GetInvoiceByIdAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity);

        _repositoryMock.Setup(r => r.InvoiceExistsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.InvoicePositionExistsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await command.Execute(_repositoryMock.Object);

        // Assert
        _repositoryMock.Verify(r => r.RemoveRangeAsync(It.IsAny<IEnumerable<InvoicePosition>>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.RemoveAsync(invoiceEntity, It.IsAny<CancellationToken>()), Times.Once);
    }
}