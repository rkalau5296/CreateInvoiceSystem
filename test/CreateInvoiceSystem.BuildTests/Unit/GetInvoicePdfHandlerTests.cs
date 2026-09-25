using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetPdf;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class GetInvoicePdfHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<IInvoiceExportService> _exportServiceMock;
    private readonly GetInvoicePdfHandler _sut;

    public GetInvoicePdfHandlerTests()
    {
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _exportServiceMock = new Mock<IInvoiceExportService>();

        _sut = new GetInvoicePdfHandler(
            _invoiceRepositoryMock.Object,
            _exportServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnPdfResponse_WhenInvoiceExists()
    {
        // Arrange
        const int invoiceId = 1;
        const int userId = 100;
        var request = new GetInvoicePdfRequest(invoiceId, userId);

        var invoiceEntity = new Invoice
        {
            InvoiceId = invoiceId,
            UserId = userId,
            Title = "FV/2026/001",
            TotalNet = 121.95m,
            TotalVat = 28.05m,
            TotalGross = 150.00m,
            ClientName = "Testowy Klient",
            InvoicePositions = new List<InvoicePosition>
            {
                new()
                {
                    ProductId = 1,
                    Quantity = 5,
                    ProductValue = 24.39m,
                    VatRate = "23"
                }
            }
        };

        var expectedPdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };

        _invoiceRepositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity);

        _exportServiceMock
            .Setup(x => x.ExportToPdfAsync(It.IsAny<InvoiceDto>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPdfBytes);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPdfBytes, result.PdfContent);
        Assert.Equal("Faktura_FV_2026_001.pdf", result.FileName);

        _invoiceRepositoryMock.Verify(
            r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()),
            Times.Once);

        _exportServiceMock.Verify(
            x => x.ExportToPdfAsync(It.IsAny<InvoiceDto>(), userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenInvoiceDoesNotExist()
    {
        // Arrange
        const int invoiceId = 999;
        const int userId = 100;
        var request = new GetInvoicePdfRequest(invoiceId, userId);

        _invoiceRepositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice)null!);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Handle(request, CancellationToken.None));

        // Assert
        Assert.Equal($"Invoice with ID {invoiceId} not found.", exception.Message);

        _invoiceRepositoryMock.Verify(
            r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()),
            Times.Once);

        _exportServiceMock.Verify(
            x => x.ExportToPdfAsync(It.IsAny<InvoiceDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}