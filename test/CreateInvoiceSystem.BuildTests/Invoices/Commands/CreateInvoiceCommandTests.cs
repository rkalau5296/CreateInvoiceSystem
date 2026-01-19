using Moq;
using FluentAssertions;
using Xunit;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;

namespace CreateInvoiceSystem.BuildTests.Invoices.Commands;

public class CreateInvoiceCommandTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly Mock<IInvoiceEmailSender> _emailSenderMock;

    public CreateInvoiceCommandTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
        _emailSenderMock = new Mock<IInvoiceEmailSender>();
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenInvoicePositionsAreEmpty()
    {
        // Arrange
        var dto = CreateBaseDto();
        dto.InvoicePositions = new List<InvoicePositionDto>();
        var command = new CreateInvoiceCommand(dto, _emailSenderMock.Object);

        // Act
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invoice must contain at least one position.");
    }

    [Fact]
    public async Task Execute_ShouldCreateInvoiceWithExistingClient_WhenClientIdIsProvided()
    {
        // Arrange
        var dto = CreateBaseDto();
        dto.ClientId = 100;
        var command = new CreateInvoiceCommand(dto, _emailSenderMock.Object);

        var existingClient = new Client { ClientId = 100, Name = "Test Client" };
        var product = new Product { ProductId = 10, Name = "Laptop", Value = 1000m };
        var invoiceEntity = new Invoice { InvoiceId = 1, UserId = dto.UserId, Title = dto.Title };

        _repositoryMock.Setup(r => r.GetClientByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        _repositoryMock.Setup(r => r.GetProductByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _repositoryMock.Setup(r => r.AddInvoiceAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity);

        _repositoryMock.Setup(r => r.GetInvoiceByIdAsync(dto.UserId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Invoice
            {
                InvoiceId = 1,
                UserId = dto.UserId,
                Client = existingClient,
                InvoicePositions = new List<InvoicePosition> { new() }
            });

        _repositoryMock.Setup(r => r.GetUserEmailByIdAsync(dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("test@user.com");

        // Act
        var result = await command.Execute(_repositoryMock.Object);

        // Assert
        result.Should().NotBeNull();
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailSenderMock.Verify(e => e.SendInvoiceCreatedEmailAsync("test@user.com", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenProductNotFoundById()
    {
        // Arrange
        var dto = CreateBaseDto();
        var command = new CreateInvoiceCommand(dto, _emailSenderMock.Object);

        _repositoryMock.Setup(r => r.GetClientByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client());

        _repositoryMock.Setup(r => r.GetProductByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null);

        // Act
        Func<Task> act = async () => await command.Execute(_repositoryMock.Object);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Product with ID 10 not found.");
    }

    private CreateInvoiceDto CreateBaseDto()
    {
        // Wykorzystanie recordu InvoicePositionDto zgodnie z nową definicją
        var position = new InvoicePositionDto(
            InvoicePositionId: 0,
            InvoiceId: 0,
            ProductId: 10,
            Product: null,
            ProductName: "Laptop",
            ProductDescription: "High-end laptop",
            ProductValue: 1000m,
            Quantity: 2
        );

        return new CreateInvoiceDto
        {
            UserId = 1,
            Title = "Invoice 2024",
            ClientId = 1,
            InvoicePositions = new List<InvoicePositionDto> { position }
        };
    }
}