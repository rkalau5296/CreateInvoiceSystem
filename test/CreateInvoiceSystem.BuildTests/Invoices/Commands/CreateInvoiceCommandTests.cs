using Moq;
using FluentAssertions;
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
        var dto = CreateBaseDto();
        dto.InvoicePositions = new List<InvoicePositionDto>();
        var command = new CreateInvoiceCommand(dto, _emailSenderMock.Object);

        Func<Task> act = async () => await command.Execute(_repositoryMock.Object);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invoice must contain at least one position.");
    }

    [Fact]
    public async Task Execute_ShouldCreateInvoiceWithExistingClient_WhenClientIdIsProvided()
    {
        // Arrange
        var dto = CreateBaseDto();
        dto.ClientId = 100;
        
        dto.InvoicePositions = new List<InvoicePositionDto>
    {
        new(0, 0, 10, null, "Laptop", "Opis", 1000m, 2, "23%")
    };

        var command = new CreateInvoiceCommand(dto, _emailSenderMock.Object);

        var existingClient = new Client
        {
            ClientId = 100,
            Name = "Test Client",
            Nip = "9876543210",
            Address = new Address { Street = "Klienta", Number = "10", City = "Kraków" }
        };

        var invoiceEntity = new Invoice
        {
            InvoiceId = 1,
            UserId = dto.UserId,
            Title = dto.Title,
            TotalNet = 2000m,
            TotalVat = 460m,
            TotalGross = 2460m,
            SellerName = "Moja Firma",
            SellerNip = "1234567890",
            SellerAddress = "Testowa 1",
            BankAccountNumber = "PL0011",
            ClientName = "Test Client",
            ClientNip = "9876543210",
            ClientAddress = "Klienta 10",
            MethodOfPayment = "Transfer",            
            Client = existingClient,
            InvoicePositions = new List<InvoicePosition>
        {
            new() { InvoicePositionId = 1, ProductName = "Laptop", ProductValue = 1000m, Quantity = 2, VatRate = "23%" }
        }
        };

        _repositoryMock.Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserId = dto.UserId, Name = "Moja Firma" });

        _repositoryMock.Setup(r => r.GetClientByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        _repositoryMock.Setup(r => r.GetProductByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { ProductId = 10, Name = "Laptop", Value = 1000m });

        _repositoryMock.Setup(r => r.AddInvoiceAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity);

        _repositoryMock.Setup(r => r.GetInvoiceByIdAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity);

        _repositoryMock.Setup(r => r.GetUserEmailByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("test@user.com");

        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await command.Execute(_repositoryMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.InvoiceId.Should().Be(1);
    }

    [Fact]
    public async Task Execute_ShouldCreateInvoiceWithNewProduct_WhenProductIdIsNull()
    {
        // Arrange
        var dto = CreateBaseDto();
        dto.ClientId = 100;
        dto.InvoicePositions = new List<InvoicePositionDto>
        {
            new(0, 0, null,
                new ProductDto(0, "Nowy Produkt", "Opis", 500m, 1, false),
                "Nowy Produkt", "Opis", 500m, 1, "23%")
        };

        var command = new CreateInvoiceCommand(dto, _emailSenderMock.Object);

        var existingClient = new Client
        {
            ClientId = 100,
            Name = "Test Client",
            Nip = "1234567890",
            Address = new Address { City = "Kraków" }
        };

        var newProductEntity = new Product
        {
            ProductId = 200,
            Name = "Nowy Produkt",
            Value = 500m,
            UserId = dto.UserId
        };

        var invoiceEntity = new Invoice
        {
            InvoiceId = 1,
            UserId = dto.UserId,
            Title = dto.Title,
            TotalNet = 500m,
            TotalVat = 115m,
            TotalGross = 615m,
            SellerName = "Moja Firma",
            ClientName = "Test Client",
            Client = existingClient,
            InvoicePositions = new List<InvoicePosition>
        {
            new() { InvoicePositionId = 1, Product = newProductEntity, ProductName = "Nowy Produkt", ProductValue = 500m, Quantity = 1, VatRate = "23%" }
        }
        };

        _repositoryMock.Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserId = dto.UserId, Name = "Moja Firma" });

        _repositoryMock.Setup(r => r.GetClientByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        _repositoryMock.Setup(r => r.GetProductAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null);

        _repositoryMock.Setup(r => r.AddProductAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.AddInvoiceAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity);

        _repositoryMock.Setup(r => r.GetInvoiceByIdAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity);

        _repositoryMock.Setup(r => r.GetUserEmailByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("test@user.com");

        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await command.Execute(_repositoryMock.Object);

        // Assert
        result.Should().NotBeNull();
        _repositoryMock.Verify(r => r.AddProductAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AddInvoiceAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenProductNotFoundById()
    {
        var dto = CreateBaseDto();
        var command = new CreateInvoiceCommand(dto, _emailSenderMock.Object);

        _repositoryMock.Setup(r => r.GetUserByIdAsync(dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                UserId = dto.UserId,
                Name = "Seller",
                Address = new Address { Street = "S", City = "C" }
            });

        _repositoryMock.Setup(r => r.GetClientByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client());

        _repositoryMock.Setup(r => r.GetProductByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        Func<Task> act = async () => await command.Execute(_repositoryMock.Object);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Product with ID 10 not found.");
    }

    private CreateInvoiceDto CreateBaseDto()
    {
        var position = new InvoicePositionDto(0, 0, 10, null!, "Laptop", "High-end laptop", 1000m, 2, "23%");

        return new CreateInvoiceDto
        {
            UserId = 1,
            Title = "Invoice 2024",
            ClientId = 1,
            InvoicePositions = new List<InvoicePositionDto> { position }
        };
    }
}