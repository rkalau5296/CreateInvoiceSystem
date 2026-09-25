using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.UpdateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class UpdateInvoiceHandlerTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly UpdateInvoiceHandler _sut;

    public UpdateInvoiceHandlerTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
        _sut = new UpdateInvoiceHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSyncPositions_RemovingAndAddingCorrectly()
    {
        // Arrange
        const int invoiceId = 1;
        const int userId = 100;

        var incomingPositions = new List<UpdateInvoicePositionDto>
        {
            new(10, invoiceId, 500, "Updated Product", "Desc", 100m, 5, "23%", null),
            new(0, invoiceId, 600, "New Product", "Desc", 200m, 1, "8%", null)
        };

        var updateDto = new UpdateInvoiceDto(
            invoiceId, "New Title", 1000m, 230m, 1230m, DateTime.Now.AddDays(7), DateTime.Now, "Updated",
            null, userId, null!, "Transfer", incomingPositions, "My Company", "9876543210", "Main St 1",
            "PL00112233", "Client Name", "123456789", "Client Address", "testc@test.com"
        );

        var request = new UpdateInvoiceRequest(invoiceId, updateDto) { UserId = userId };

        var existingInvoice = new Invoice
        {
            InvoiceId = invoiceId,
            UserId = userId,
            Title = "Old Title",
            InvoicePositions = new List<InvoicePosition>
            {
                new() { InvoicePositionId = 10, ProductName = "Old Product", Quantity = 1 },
                new() { InvoicePositionId = 20, ProductName = "To Delete", Quantity = 1 }
            }
        };

        _repositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvoice);

        _repositoryMock
            .Setup(r => r.GetProductAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { ProductId = 99, Name = "Some Product" });

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.InvoiceId.Should().Be(invoiceId);

        _repositoryMock.Verify(r => r.RemoveInvoicePositionsAsync(It.Is<InvoicePosition>(p => p.InvoicePositionId == 20)), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenNoChangesDetected()
    {
        // Arrange
        const int userId = 1;
        const int invoiceId = 1;
        var now = new DateTime(2026, 1, 1);

        var updateDto = new UpdateInvoiceDto(
            invoiceId, "Same", 100m, 23m, 123m, now, now, "Same", null, userId, null!, "Card",
            new List<UpdateInvoicePositionDto>(), "Seller", "NIP", "Address", "Bank", "Client", "C-NIP", "C-ADDR", "testc@test.com"
        );
        var request = new UpdateInvoiceRequest(invoiceId, updateDto) { UserId = userId };

        var invoiceEntity = new Invoice
        {
            UserId = userId,
            InvoiceId = invoiceId,
            Title = "Same",
            TotalNet = 100m,
            TotalVat = 23m,
            TotalGross = 123m,
            PaymentDate = now,
            CreatedDate = now,
            Comments = "Same",
            MethodOfPayment = "Card",
            SellerName = "Seller",
            SellerNip = "NIP",
            SellerAddress = "Address",
            BankAccountNumber = "Bank",
            ClientName = "Client",
            ClientNip = "C-NIP",
            ClientAddress = "C-ADDR",
            ClientId = null,
            InvoicePositions = new List<InvoicePosition>()
        };

        _repositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.InvoiceId.Should().Be(invoiceId);
        result.Data.Title.Should().Be("Same");
        result.Data.TotalNet.Should().Be(100m);
        result.Data.TotalVat.Should().Be(23m);
        result.Data.TotalGross.Should().Be(123m);
        result.Data.MethodOfPayment.Should().Be("Card");
        result.Data.SellerName.Should().Be("Seller");
        result.Data.ClientName.Should().Be("Client");
        result.Data.InvoicePositions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldUpdateClientSnapshot_WhenNewClientIdIsProvided()
    {
        // Arrange
        const int userId = 1;
        const int invoiceId = 1;
        const int clientId = 500;

        var updateDto = new UpdateInvoiceDto(
            invoiceId, "T", 100m, 23m, 123m, DateTime.Now, DateTime.Now, "C",
            clientId, userId, null!, "Card", new List<UpdateInvoicePositionDto>(),
            "S", "SN", "SA", "SB", "CN", "CNIP", "CADDR", "testc@test.com"
        );

        var request = new UpdateInvoiceRequest(invoiceId, updateDto) { UserId = userId };

        var invoice = new Invoice
        {
            UserId = userId,
            InvoiceId = invoiceId,
            ClientId = 10,
            ClientName = "Old Corp",
            InvoicePositions = new List<InvoicePosition>()
        };

        var existingClient = new Client
        {
            ClientId = clientId,
            UserId = userId,
            Name = "New Corp",
            Nip = "9876543210",
            Email = "new@test.com",
            Address = new Address
            {
                Street = "S",
                Number = "1",
                City = "C",
                PostalCode = "0",
                Country = "PL"
            }
        };

        _repositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _repositoryMock
            .Setup(r => r.GetClientByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        invoice.ClientId.Should().Be(500);
        invoice.ClientName.Should().Be("New Corp");
        invoice.ClientNip.Should().Be("9876543210");
        invoice.ClientEmail.Should().Be("new@test.com");
        invoice.ClientAddress.Should().Be("S 1, 0 C, PL");

        _repositoryMock.Verify(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.GetClientByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);

        _repositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Invoice>(updatedInvoice =>
                    updatedInvoice.ClientName == "New Corp"
                    && updatedInvoice.ClientNip == "9876543210"
                    && updatedInvoice.ClientEmail == "new@test.com"
                    && updatedInvoice.ClientAddress == "S 1, 0 C, PL"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenInvoiceNotFound()
    {
        // Arrange
        const int userId = 1;
        const int invoiceId = 999;

        var updateDto = new UpdateInvoiceDto(
            invoiceId, "T", 100m, 23m, 123m, DateTime.Now, DateTime.Now, "C", 1, userId, null!, "Card",
            new List<UpdateInvoicePositionDto>(), "S", "SN", "SA", "SB", "CN", "CNIP", "CADDR", "testc@test.com"
        );

        var request = new UpdateInvoiceRequest(invoiceId, updateDto) { UserId = userId };

        _repositoryMock
            .Setup(r => r.GetInvoiceByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Invoice {invoiceId} not found.");
    }
}