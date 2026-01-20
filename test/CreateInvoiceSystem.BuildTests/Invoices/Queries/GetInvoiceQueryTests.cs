using Moq;
using FluentAssertions;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;

namespace CreateInvoiceSystem.BuildTests.Invoices.Queries;

public class GetInvoiceQueryTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;

    public GetInvoiceQueryTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
    }

    [Fact]
    public async Task Execute_ShouldReturnInvoice_WhenInvoiceExists()
    {
        // Arrange
        var userId = 1;
        var invoiceId = 10;
        var query = new GetInvoiceQuery(userId, invoiceId);
        var expectedInvoice = new Invoice { InvoiceId = invoiceId, UserId = userId, Title = "Test Invoice" };

        _repositoryMock.Setup(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInvoice);

        // Act
        var result = await query.Execute(_repositoryMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.InvoiceId.Should().Be(invoiceId);
        result.UserId.Should().Be(userId);
        _repositoryMock.Verify(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenInvoiceDoesNotExist()
    {
        // Arrange
        var userId = 1;
        var invoiceId = 999;
        var query = new GetInvoiceQuery(userId, invoiceId);

        _repositoryMock.Setup(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice)null);

        // Act
        Func<Task> act = async () => await query.Execute(_repositoryMock.Object);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Invoice with ID {invoiceId} not found.");
    }
}