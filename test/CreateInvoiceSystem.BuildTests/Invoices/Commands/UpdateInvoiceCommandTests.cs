using Moq;
using FluentAssertions;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;

namespace CreateInvoiceSystem.BuildTests.Invoices.Commands;

public class UpdateInvoiceCommandTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;

    public UpdateInvoiceCommandTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
    }

    [Fact]
    public async Task Execute_ShouldSyncPositions_RemovingAndAddingCorrectly()
    {
        // Arrange
        var invoiceId = 1;
        var userId = 100;

        var incomingPositions = new List<UpdateInvoicePositionDto>
        {
            new UpdateInvoicePositionDto(10, invoiceId, 500, "Updated Product", "Desc", 100m, 5, null),
            new UpdateInvoicePositionDto(0, invoiceId, 600, "New Product", "Desc", 200m, 1, null)
        };

        var updateDto = new UpdateInvoiceDto(
            invoiceId,
            "New Title",
            1000m,
            DateTime.Now.AddDays(7),
            DateTime.Now,
            "Updated",
            null,
            userId,
            null,
            "Transfer",
            incomingPositions,
            "Client",
            "123",
            "Addr"
        );

        var command = new UpdateInvoiceCommand { Parametr = updateDto };

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

        var updatedInvoiceEntity = new Invoice
        {
            InvoiceId = invoiceId,
            Title = "New Title",
            InvoicePositions = new List<InvoicePosition>
            {
                new() { ProductName = "Updated Product", Quantity = 5, ProductValue = 100m }
            }
        };

        _repositoryMock.SetupSequence(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvoice)
            .ReturnsAsync(existingInvoice)
            .ReturnsAsync(updatedInvoiceEntity);

        _repositoryMock.Setup(r => r.GetProductAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { ProductId = 99, Name = "Some Product" });

        // Act
        var result = await command.Execute(_repositoryMock.Object);

        // Assert
        result.Should().NotBeNull();
        _repositoryMock.Verify(r => r.RemoveInvoicePositionsAsync(It.Is<InvoicePosition>(p => p.InvoicePositionId == 20)), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenNoChangesDetected()
    {
        // Arrange
        var userId = 1;
        var invoiceId = 1;
        var now = new DateTime(2026, 1, 1);
        
        var updateDto = new UpdateInvoiceDto(
            invoiceId, "Same", 100m, now, now, "Same", null, userId, null, "Card", new List<UpdateInvoicePositionDto>(), "N", "1", "A"
        );
        var command = new UpdateInvoiceCommand { Parametr = updateDto };

        var invoiceEntity = new Invoice
        {
            UserId = userId,
            InvoiceId = invoiceId,
            Title = "Same",
            TotalAmount = 100m,
            PaymentDate = now,
            CreatedDate = now,
            Comments = "Same",
            MethodOfPayment = "Card",
            ClientName = "N",
            ClientNip = "1",
            ClientAddress = "A",
            ClientId = null,
            InvoicePositions = new List<InvoicePosition>()
        };
        
        _repositoryMock.Setup(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity);

        // Act
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("The invoice has not changed.");
    }

    [Fact]
    public async Task Execute_ShouldUpdateClient_WhenNewClientIdIsProvided()
    {
        // Arrange
        var userId = 1;
        var invoiceId = 1;
        var newClientId = 500;

        var updateDto = new UpdateInvoiceDto(
            invoiceId, "T", 100m, DateTime.Now, DateTime.Now, "C", newClientId, userId, null, "Card", new List<UpdateInvoicePositionDto>(), "CN", "NIP", "ADDR"
        );
        var command = new UpdateInvoiceCommand { Parametr = updateDto };

        var invoice = new Invoice { UserId = userId, InvoiceId = invoiceId, ClientId = 10, InvoicePositions = new List<InvoicePosition>() };
        var newClient = new Client
        {
            ClientId = newClientId,
            Name = "New Corp",
            Address = new Address { Street = "S", Number = "1", City = "C", PostalCode = "0", Country = "PL" }
        };

        _repositoryMock.SetupSequence(r => r.GetInvoiceByIdAsync(userId, invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice)
            .ReturnsAsync(invoice)
            .ReturnsAsync(new Invoice { Title = "Changed" });

        _repositoryMock.Setup(r => r.GetClientByIdAsync(newClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newClient);

        // Act
        await command.Execute(_repositoryMock.Object);

        // Assert
        invoice.ClientId.Should().Be(newClientId);
        invoice.ClientName.Should().Be("New Corp");
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenInvoiceNotFound()
    {
        // Arrange
        var updateDto = new UpdateInvoiceDto(
            999, "T", 100m, DateTime.Now, DateTime.Now, "C", 1, 1, null, "Card", new List<UpdateInvoicePositionDto>(), "N", "1", "A"
        );
        var command = new UpdateInvoiceCommand { Parametr = updateDto };

        _repositoryMock.Setup(r => r.GetInvoiceByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice)null);

        // Act
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}