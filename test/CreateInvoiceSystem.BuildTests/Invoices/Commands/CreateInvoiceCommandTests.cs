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

        // Globalny setup dla numeracji: udajemy, że to zawsze pierwsza faktura w miesiącu
        _repositoryMock.Setup(r => r.GetInvoicesCountInMonthAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
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
    public async Task Execute_ShouldThrowException_WhenVatRateIsInvalid()
    {
        var dto = CreateBaseDto();
        dto.InvoicePositions = new List<InvoicePositionDto>
        {
            new(0, 0, 10, null, "Laptop", "Opis", 1000m, 2, "123%")
        };

        var command = new CreateInvoiceCommand(dto, _emailSenderMock.Object);

        var act = () => command.Execute(_repositoryMock.Object);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid VatRate: 123%. Allowed values are: 23%, 8%, 5%, 0%, zw, np");
    }

    [Fact]
    public async Task Execute_ShouldCreateInvoiceWithExistingClient_WhenClientIdIsProvided()
    {
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

        _repositoryMock.Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserId = dto.UserId, Name = "Moja Firma" });

        _repositoryMock.Setup(r => r.GetClientByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        _repositoryMock.Setup(r => r.GetProductByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { ProductId = 10, Name = "Laptop", Value = 1000m });

        // Zwracamy obiekt przekazany do metody (z wyliczonymi sumami i numerem)
        _repositoryMock.Setup(r => r.AddInvoiceAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice inv, CancellationToken ct) => inv);

        _repositoryMock.Setup(r => r.GetInvoiceByIdAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int? u, int id, CancellationToken ct) => {
                var inv = new Invoice
                {
                    InvoiceId = id,
                    UserId = u ?? 0,
                    Client = existingClient,
                    InvoicePositions = new List<InvoicePosition> {
                        new() { ProductValue = 1000m, Quantity = 2, VatRate = "23%" }
                    }
                };
                inv.RecalculateTotals();
                inv.Title = "1/01/2026";
                return inv;
            });

        _repositoryMock.Setup(r => r.GetUserEmailByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("test@user.com");

        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await command.Execute(_repositoryMock.Object);

        result.Should().NotBeNull();
        result.TotalNet.Should().Be(2000m);
        result.TotalVat.Should().Be(460m);
        result.TotalGross.Should().Be(2460m);
        result.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Execute_ShouldCreateInvoiceWithNewProduct_WhenProductIdIsNull()
    {
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

        _repositoryMock.Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserId = dto.UserId, Name = "Moja Firma" });

        _repositoryMock.Setup(r => r.GetClientByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        _repositoryMock.Setup(r => r.GetProductAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        _repositoryMock.Setup(r => r.AddProductAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock.Setup(r => r.AddInvoiceAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice inv, CancellationToken ct) => inv);

        _repositoryMock.Setup(r => r.GetInvoiceByIdAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int? u, int id, CancellationToken ct) => {
                var inv = new Invoice
                {
                    InvoiceId = id,
                    UserId = u ?? 0,
                    Client = existingClient,
                    InvoicePositions = new List<InvoicePosition> {
                        new() { ProductValue = 500m, Quantity = 1, VatRate = "23%" }
                    }
                };
                inv.RecalculateTotals();
                inv.Title = "1/01/2026";
                return inv;
            });

        _repositoryMock.Setup(r => r.GetUserEmailByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("test@user.com");

        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await command.Execute(_repositoryMock.Object);

        result.Should().NotBeNull();
        result.TotalGross.Should().Be(615m);
        _repositoryMock.Verify(r => r.AddProductAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenProductNotFoundById()
    {
        var dto = CreateBaseDto();
        var command = new CreateInvoiceCommand(dto, _emailSenderMock.Object);

        _repositoryMock.Setup(r => r.GetUserByIdAsync(dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserId = dto.UserId, Name = "Seller" });

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