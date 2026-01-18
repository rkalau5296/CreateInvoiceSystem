using CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Products.Commands;

public class CreateProductCommandTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly CreateProductCommand _command;

    public CreateProductCommandTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _command = new CreateProductCommand();
    }

    [Fact]
    public async Task Execute_ShouldSaveProduct_WhenNameIsUnique()
    {
        // Arrange
        var dto = new CreateProductDto("Unikalny Produkt", "Opis", 100m, 1);
        _command.Parametr = dto;

        var entity = new Product { ProductId = 50, Name = "Unikalny Produkt", UserId = 1 };
        
        _repositoryMock.Setup(r => r.ExistsAsync(dto.Name, dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(entity.ProductId, entity.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Unikalny Produkt");

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidOperationException_WhenProductAlreadyExists()
    {
        // Arrange
        var dto = new CreateProductDto("Istniejący Produkt", "Opis", 10m, 1);
        _command.Parametr = dto;

        _repositoryMock.Setup(r => r.ExistsAsync(dto.Name, dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The product with the same name already exists.");

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenProductCouldNotBeReloaded()
    {
        // Arrange
        var dto = new CreateProductDto("Produkt Widmo", "Opis", 10m, 1);
        _command.Parametr = dto;
        var entity = new Product { ProductId = 99, Name = "Produkt Widmo", UserId = 1 };

        _repositoryMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        // Act
        Func<Task> act = async () => await _command.Execute(_repositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Product was saved but could not be reloaded.");
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