using CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.DeleteProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class DeleteProductHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly DeleteProductHandler _sut;

    public DeleteProductHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _sut = new DeleteProductHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnProductDto_WhenProductIsSuccessfullyDeleted()
    {
        // Arrange
        const int productId = 10;
        const int userId = 1;
        var request = new DeleteProductRequest(productId) { UserId = userId };

        var existingProduct = new Product
        {
            ProductId = productId,
            Name = "Produkt do usunięcia",
            Description = "Opis produktu",
            Value = 150.50m,
            UserId = userId
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data!.ProductId.Should().Be(productId);
        result.Data.Name.Should().Be("Produkt do usunięcia");
        result.Data.Description.Should().Be("Opis produktu");
        result.Data.Value.Should().Be(150.50m);
        result.Data.UserId.Should().Be(userId);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(productId, userId, It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            r => r.RemoveAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenProductDoesNotExist()
    {
        // Arrange
        const int productId = 99;
        const int userId = 1;
        var request = new DeleteProductRequest(productId) { UserId = userId };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Product with ID {productId} not found.");

        _repositoryMock.Verify(
            r => r.RemoveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRemoveFails()
    {
        // Arrange
        const int productId = 5;
        const int userId = 1;
        var request = new DeleteProductRequest(productId) { UserId = userId };

        var existingProduct = new Product
        {
            ProductId = productId,
            Name = "Produkt",
            UserId = userId
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock
            .Setup(r => r.RemoveAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException($"Failed to delete Product with ID {productId}."));

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Failed to delete Product with ID {productId}.");

        _repositoryMock.Verify(
            r => r.GetByIdAsync(productId, userId, It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            r => r.RemoveAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenIdIsLessThanOne()
    {
        // Act
        Action act = () => new DeleteProductRequest(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("id");
    }
}