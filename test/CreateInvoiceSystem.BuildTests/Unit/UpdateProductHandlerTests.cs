using CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.UpdateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class UpdateProductHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly UpdateProductHandler _sut;

    public UpdateProductHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _sut = new UpdateProductHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProductFields_AndReturnUpdatedDto()
    {
        // Arrange
        const int productId = 10;
        const int userId = 100;

        var inputDto = new UpdateProductDto(productId, "Nowa Nazwa", "Nowy Opis", 200m, userId);
        var request = new UpdateProductRequest(productId, inputDto);

        var existingProduct = new Product
        {
            ProductId = productId,
            UserId = userId,
            Name = "Stara Nazwa",
            Description = "Stary Opis",
            Value = 100m
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data!.ProductId.Should().Be(productId);
        result.Data.Name.Should().Be("Nowa Nazwa");
        result.Data.Description.Should().Be("Nowy Opis");
        result.Data.Value.Should().Be(200m);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(productId, It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Product>(p =>
                    p.ProductId == productId
                    && p.UserId == userId
                    && p.Name == "Nowa Nazwa"
                    && p.Description == "Nowy Opis"
                    && p.Value == 200m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenProductNotFound()
    {
        // Arrange
        const int productId = 99;
        const int userId = 1;

        var inputDto = new UpdateProductDto(productId, "Test", "Test", 10m, userId);
        var request = new UpdateProductRequest(productId, inputDto);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Product with ID {productId} not found.");

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMaintainOldValue_WhenInputFieldsAreNull()
    {
        // Arrange
        const int productId = 1;
        const int userId = 100;

        var inputDto = new UpdateProductDto(productId, null!, null!, null, userId);
        var request = new UpdateProductRequest(productId, inputDto);

        var existingProduct = new Product
        {
            ProductId = productId,
            UserId = userId,
            Name = "Zachowaj Mnie",
            Description = "Stary Opis",
            Value = 50m
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert 
        _repositoryMock.Verify(
            r => r.UpdateAsync(
                It.Is<Product>(p =>
                    p.Name == "Zachowaj Mnie"
                    && p.Description == "Stary Opis"
                    && p.Value == 50m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenUpdateAsyncFails()
    {
        // Arrange
        const int productId = 1;
        const int userId = 100;

        var inputDto = new UpdateProductDto(productId, "Nowa Nazwa", "Opis", 100m, userId);
        var request = new UpdateProductRequest(productId, inputDto);

        var existingProduct = new Product { ProductId = productId, UserId = userId };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error."));

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error.");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDtoIsNull()
    {
        // Act
        Action act = () => new UpdateProductRequest(1, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*productDto*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenIdIsZero()
    {
        // Arrange
        var dto = new UpdateProductDto(0, "Test", "Test", 10m, 1);

        // Act
        Action act = () => new UpdateProductRequest(0, dto);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}