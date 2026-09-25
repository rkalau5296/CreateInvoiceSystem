using CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.CreateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly CreateProductHandler _sut;

    public CreateProductHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _sut = new CreateProductHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSaveProductAndReturnDto_WhenNameIsUnique()
    {
        // Arrange
        var dto = new CreateProductDto(
            "Unikalny Produkt",
            "Opis",
            100m,
            1);

        var request = new CreateProductRequest(dto);

        var savedEntity = new Product
        {
            ProductId = 50,
            Name = dto.Name,
            Description = dto.Description,
            Value = dto.Value,
            UserId = dto.UserId
        };

        _repositoryMock
            .Setup(r => r.ExistsAsync(
                dto.Name,
                dto.UserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntity);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(dto.Name);
        result.Data.Description.Should().Be(dto.Description);
        result.Data.Value.Should().Be(dto.Value);
        result.Data.UserId.Should().Be(dto.UserId);

        _repositoryMock.Verify(
            r => r.ExistsAsync(
                dto.Name,
                dto.UserId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            r => r.AddAsync(
                It.Is<Product>(p =>
                    p.Name == dto.Name
                    && p.Description == dto.Description
                    && p.Value == dto.Value
                    && p.UserId == dto.UserId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOperationException_WhenProductAlreadyExists()
    {
        // Arrange
        var dto = new CreateProductDto("Istniejący Produkt", "Opis", 10m, 1);
        var request = new CreateProductRequest(dto);

        _repositoryMock
            .Setup(r => r.ExistsAsync(dto.Name, dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Istnieje już produkt o takiej nazwie.");

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenProductInRequestIsNull()
    {
        // Arrange
        var request = new CreateProductRequest(null!);

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();

        _repositoryMock.Verify(
            r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenAddAsyncFails()
    {
        // Arrange
        var dto = new CreateProductDto("Nowy Produkt", "Opis", 50m, 1);
        var request = new CreateProductRequest(dto);

        _repositoryMock
            .Setup(r => r.ExistsAsync(dto.Name, dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Błąd zapisu w bazie danych."));

        // Act
        Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Błąd zapisu w bazie danych.");
    }
}