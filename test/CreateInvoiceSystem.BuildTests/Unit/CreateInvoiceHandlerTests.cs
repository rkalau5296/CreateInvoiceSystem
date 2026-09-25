using System.Threading.Channels;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.CreateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.BackgroundTasks;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class CreateInvoiceHandlerTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly Mock<ChannelWriter<EmailTask>> _writerMock;
    private readonly CreateInvoiceHandler _sut;

    public CreateInvoiceHandlerTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
        _writerMock = new Mock<ChannelWriter<EmailTask>>();

        _repositoryMock
            .Setup(r => r.GetMaxInvoiceNumberInMonthAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _sut = new CreateInvoiceHandler(_repositoryMock.Object, _writerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenInvoicePositionsAreEmpty()
    {
        // Arrange
        var dto = CreateBaseDto();
        dto.InvoicePositions = new List<InvoicePositionDto>();
        var request = new CreateInvoiceRequest(dto);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invoice must contain at least one position.");

        _writerMock.Verify(w => w.WriteAsync(It.IsAny<EmailTask>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenVatRateIsInvalid()
    {
        // Arrange
        var dto = CreateBaseDto();
        dto.InvoicePositions = new List<InvoicePositionDto>
        {
            new(0, 0, 10, null!, "Laptop", "Opis", 1000m, 2, "123%")
        };
        var request = new CreateInvoiceRequest(dto);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid VatRate: 123%. Allowed values are: 23%, 8%, 5%, 0%, zw, np");

        _writerMock.Verify(w => w.WriteAsync(It.IsAny<EmailTask>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateInvoiceWithExistingClient_WhenClientIdIsProvided()
    {
        // Arrange
        var dto = CreateBaseDto();
        dto.ClientId = 100;
        dto.InvoicePositions = new List<InvoicePositionDto>
        {
            new(0, 0, 10, null!, "Laptop", "Opis", 1000m, 2, "23%")
        };
        var request = new CreateInvoiceRequest(dto);

        var existingClient = new Client
        {
            ClientId = 100,
            Name = "Test Client",
            Nip = "9876543210",
            Email = "client@test.com",
            Address = new Address { Street = "Klienta", Number = "10", City = "Kraków" }
        };

        _repositoryMock.Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserId = dto.UserId, Name = "Moja Firma" });

        _repositoryMock.Setup(r => r.GetClientByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        _repositoryMock.Setup(r => r.GetProductByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { ProductId = 10, Name = "Laptop", Value = 1000m });

        _repositoryMock.Setup(r => r.AddInvoiceAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice inv, CancellationToken ct) => inv);

        _repositoryMock.Setup(r => r.GetUserEmailByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("test@user.com");

        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.TotalNet.Should().Be(2000m);
        result.Data.TotalVat.Should().Be(460m);
        result.Data.TotalGross.Should().Be(2460m);
        result.Data.Title.Should().NotBeNullOrEmpty();

        _writerMock.Verify(w => w.WriteAsync(It.Is<SellerEmailTask>(task => task.UserEmail == "test@user.com"), It.IsAny<CancellationToken>()), Times.Once);
        _writerMock.Verify(w => w.WriteAsync(It.Is<ClientEmailTask>(task => task.Invoice.ClientEmail == "client@test.com"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateInvoiceWithNewProduct_WhenProductIdIsNull()
    {
        // Arrange
        var dto = CreateBaseDto();
        dto.ClientId = 100;
        dto.InvoicePositions = new List<InvoicePositionDto>
        {
            new(0, 0, null,
                new ProductDto(0, "Nowy Produkt", "Opis", 500m, 1),
                "Nowy Produkt", "Opis", 500m, 1, "23%")
        };
        var request = new CreateInvoiceRequest(dto);

        var existingClient = new Client
        {
            ClientId = 100,
            Name = "Test Client",
            Nip = "1234567890",
            Email = "client@test.com",
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

        _repositoryMock.Setup(r => r.GetUserEmailByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("test@user.com");

        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.TotalGross.Should().Be(615m);

        _repositoryMock.Verify(r => r.AddProductAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _writerMock.Verify(w => w.WriteAsync(It.Is<SellerEmailTask>(task => task.UserEmail == "test@user.com"), It.IsAny<CancellationToken>()), Times.Once);
        _writerMock.Verify(w => w.WriteAsync(It.Is<ClientEmailTask>(task => task.Invoice.ClientEmail == "client@test.com"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenProductNotFoundById()
    {
        // Arrange
        var dto = CreateBaseDto();
        var request = new CreateInvoiceRequest(dto);

        _repositoryMock.Setup(r => r.GetUserByIdAsync(dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { UserId = dto.UserId, Name = "Seller" });

        _repositoryMock.Setup(r => r.GetClientByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client());

        _repositoryMock.Setup(r => r.GetProductByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Product with ID 10 not found.");

        _writerMock.Verify(w => w.WriteAsync(It.IsAny<EmailTask>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static CreateInvoiceDto CreateBaseDto()
    {
        var position = new InvoicePositionDto(0, 0, 10, null!, "Laptop", "High-end laptop", 1000m, 2, "23%");

        return new CreateInvoiceDto
        {
            UserId = 1,
            Title = "Invoice 2026",
            ClientId = 1,
            InvoicePositions = new List<InvoicePositionDto> { position }
        };
    }
}