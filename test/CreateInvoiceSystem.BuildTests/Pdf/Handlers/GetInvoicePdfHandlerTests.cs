using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetPdf;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Pdf.Handlers;

public class GetInvoicePdfHandlerTests
{
    private readonly Mock<IQueryExecutor> _queryExecutorMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<IInvoiceExportService> _exportServiceMock;
    private readonly GetInvoicePdfHandler _handler;

    public GetInvoicePdfHandlerTests()
    {
        _queryExecutorMock = new Mock<IQueryExecutor>();
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _exportServiceMock = new Mock<IInvoiceExportService>();

        _handler = new GetInvoicePdfHandler(
            _queryExecutorMock.Object,
            _invoiceRepositoryMock.Object,
            _exportServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnPdfResponse_WhenInvoiceExists()
    {
        // Arrange
        var invoiceId = 1;
        var userId = 100;
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

        _queryExecutorMock
            .Setup(x => x.Execute(It.IsAny<GetInvoiceQuery>(), _invoiceRepositoryMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity);

        var expectedPdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };

        _exportServiceMock
            .Setup(x => x.ExportToPdfAsync(It.IsAny<InvoiceDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPdfBytes);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPdfBytes, result.PdfContent);
        Assert.Equal("Faktura_FV_2026_001.pdf", result.FileName);

        _exportServiceMock.Verify(x => x.ExportToPdfAsync(It.IsAny<InvoiceDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenInvoiceDoesNotExist()
    {
        // Arrange
        var request = new GetInvoicePdfRequest(999, 100);

        _queryExecutorMock
            .Setup(x => x.Execute(It.IsAny<GetInvoiceQuery>(), _invoiceRepositoryMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}