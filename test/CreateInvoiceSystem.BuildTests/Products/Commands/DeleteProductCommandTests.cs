using CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Products.Commands;

public class DeleteProductCommandTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly DeleteProductCommand _command;

    public DeleteProductCommandTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _command = new DeleteProductCommand();
    }

    [Fact]
    public async Task Execute_ShouldReturnProductDto_WhenProductIsSuccessfullyDeleted()
    {
        // Arrange
        var productId = 10;
        var userId = 1;
        var productParam = new Product { ProductId = productId, UserId = userId };
        _command.Parametr = productParam;

        var existingProduct = new Product
        {
            ProductId = productId,
            Name = "Do usunięcia",
            UserId = userId,
            IsDeleted = false
        };
        
        _repositoryMock.Setup(r => r.GetByIdAsync(productId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        
        _repositoryMock.Setup(r => r.ExistsByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProductId.Should().Be(productId);

        _repositoryMock.Verify(r => r.RemoveAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenProductDoesNotExist()
    {
        // Arrange
        var productParam = new Product { ProductId = 99, UserId = 1 };
        _command.Parametr = productParam;
        
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        // Act
        Func<Task> act = async () => await _command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Product with ID {productParam.ProductId} not found.");

        _repositoryMock.Verify(r => r.RemoveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenDeleteFails()
    {
        // Arrange
        var productId = 5;
        var productParam = new Product { ProductId = productId, UserId = 1 };
        _command.Parametr = productParam;

        var existingProduct = new Product { ProductId = productId, UserId = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        
        _repositoryMock.Setup(r => r.ExistsByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Failed to delete Product with ID {productId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowArgumentNullException_WhenParametrIsNull()
    {
        // Arrange
        _command.Parametr = null!;

        // Act
        Func<Task> act = async () => await _command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}