using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.CreateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using FluentValidation.TestHelper;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Products;

public class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Product_Is_Null()
    {
        // Arrange
        var request = new CreateProductRequest(null!);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Product)
            .WithErrorMessage("Product data cannot be null.");
    }

    [Fact]
    public void Should_NotHaveValidationErrors_WhenRequestIsValid()
    {
        // Arrange
        var dto = new CreateProductDto("Nowy Produkt", "Opis", 99.99m, 1);
        var request = new CreateProductRequest(dto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveValidationError_WhenProductIsNull()
    {
        // Arrange
        var request = new CreateProductRequest(null!);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Product)
            .WithErrorMessage("Product data cannot be null.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_HaveValidationError_WhenNameIsEmpty(string? invalidName)
    {
        // Arrange
        var dto = new CreateProductDto(invalidName!, "Opis", 50.00m, 1);
        var request = new CreateProductRequest(dto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Product.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Should_HaveValidationError_WhenValueHasMoreThanTwoDecimalPlaces()
    {
        // Arrange
        var dto = new CreateProductDto("Produkt", "Opis", 10.1234m, 1);
        var request = new CreateProductRequest(dto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Product.Value)
            .WithErrorMessage("Value must be a decimal with max 2 digits after the decimal point.");
    }
}