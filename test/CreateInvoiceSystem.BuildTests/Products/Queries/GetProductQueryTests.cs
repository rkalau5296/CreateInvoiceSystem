using CreateInvoiceSystem.Modules.Products.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Products.Queries;

public class GetProductQueryTests
{
    private readonly Mock<IProductRepository> _repositoryMock;

    public GetProductQueryTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
    }

    [Fact]
    public async Task Execute_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var productId = 1;
        var userId = 100;
        var query = new GetProductQuery(productId, userId);

        var expectedProduct = new Product
        {
            ProductId = productId,
            Name = "Testowy Produkt",
            UserId = userId
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProductId.Should().Be(productId);
        result.Name.Should().Be("Testowy Produkt");

        _repositoryMock.Verify(r => r.GetByIdAsync(productId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenProductNotFound()
    {
        // Arrange
        var productId = 99;
        var userId = 1;
        var query = new GetProductQuery(productId, userId);
        
        _repositoryMock
            .Setup(r => r.GetByIdAsync(productId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        // Act
        Func<Task> act = async () => await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Product with ID {productId} not found.");
    }

    [Fact]
    public async Task Execute_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var query = new GetProductQuery(1, 1);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int?>(), cts.Token))
            .ReturnsAsync(new Product());

        // Act
        await query.Execute(_repositoryMock.Object, cts.Token);

        // Assert
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int?>(), cts.Token), Times.Once);
    }
}