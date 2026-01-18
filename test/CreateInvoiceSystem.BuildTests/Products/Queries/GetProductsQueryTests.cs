using CreateInvoiceSystem.Modules.Products.Domain.Application.Queries;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Products.Queries;

public class GetProductsQueryTests
{
    private readonly Mock<IProductRepository> _repositoryMock;

    public GetProductsQueryTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
    }

    [Fact]
    public async Task Execute_ShouldReturnListOfProducts_WhenProductsExist()
    {
        // Arrange
        var userId = 123;
        var query = new GetProductsQuery(userId);
        var expectedProducts = new List<Product>
        {
            new() { ProductId = 1, Name = "Produkt A", UserId = userId },
            new() { ProductId = 2, Name = "Produkt B", UserId = userId }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);

        // Act
        var result = await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedProducts);

        _repositoryMock.Verify(r => r.GetAllAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenRepositoryReturnsNull()
    {
        // Arrange
        var userId = 1;
        var query = new GetProductsQuery(userId);
        
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Product>)null!);

        // Act
        Func<Task> act = async () => await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("List of products is empty.");
    }

    [Fact]
    public async Task Execute_ShouldReturnEmptyList_WhenRepositoryReturnsEmptyList()
    {
        // Arrange
        var query = new GetProductsQuery(null);
        var emptyList = new List<Product>();

        _repositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert        
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_ShouldHandleNullUserId()
    {
        // Arrange
        var query = new GetProductsQuery(null);
        _repositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { new() { ProductId = 1 } });

        // Act
        await query.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }
}