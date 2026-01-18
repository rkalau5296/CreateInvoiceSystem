using CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Products.Commands;

public class UpdateProductCommandTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly UpdateProductCommand _command;

    public UpdateProductCommandTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _command = new UpdateProductCommand();
    }

    [Fact]
    public async Task Execute_ShouldUpdateProductFields_AndReturnDto()
    {
        // Arrange
        var productId = 1;
        var userId = 100;
        var inputDto = new UpdateProductDto(productId, "Nowa Nazwa", "Nowy Opis", 200m, userId, false);
        _command.Parametr = inputDto;

        var existingProduct = new Product
        {
            ProductId = productId,
            UserId = userId,
            Name = "Stara Nazwa",
            Description = "Stary Opis",
            Value = 100m
        };

        var updatedEntity = new Product
        {
            ProductId = productId,
            UserId = userId,
            Name = "Nowa Nazwa",
            Description = "Nowy Opis",
            Value = 200m
        };
        
        _repositoryMock.Setup(r => r.GetByIdAsync(productId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEntity);
        
        _repositoryMock.SetupSequence(r => r.GetByIdAsync(productId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct) 
            .ReturnsAsync(updatedEntity);  

        // Act
        var result = await _command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Nowa Nazwa");
        result.Value.Should().Be(200m);

        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Product>(p =>
            p.Name == "Nowa Nazwa" && p.Value == 200m), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenProductNotFound()
    {
        // Arrange
        var inputDto = new UpdateProductDto(99, "Test", "Test", 10m, 1, false);
        _command.Parametr = inputDto;

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        // Act
        Func<Task> act = async () => await _command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Product with ID {inputDto.ProductId} not found.");
    }

    [Fact]
    public async Task Execute_ShouldMaintainOldValue_WhenInputFieldsAreNull()
    {
        // Arrange
        var productId = 1;
        var userId = 100;
        
        var inputDto = new UpdateProductDto(productId, null!, null!, null, userId, false);
        _command.Parametr = inputDto;

        var existingProduct = new Product
        {
            ProductId = productId,
            UserId = userId,
            Name = "Zachowaj Mnie",
            Description = "Opis",
            Value = 50m
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(productId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        await _command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert 
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Product>(p =>
            p.Name == "Zachowaj Mnie"), It.IsAny<CancellationToken>()), Times.Once);
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