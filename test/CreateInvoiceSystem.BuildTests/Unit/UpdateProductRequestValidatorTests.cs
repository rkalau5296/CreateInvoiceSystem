using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.UpdateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using FluentValidation.TestHelper;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class UpdateProductRequestValidatorTests
{
    private readonly UpdateProductRequestValidator _validator = new();

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        // Arrange
        var dto = new UpdateProductDto(1, "Valid Name", "Short Desc", 49.99m, 1);
        var request = new UpdateProductRequest(1, dto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Have_Error_When_Id_Is_Less_Than_One(int invalidId)
    {
        // Arrange
        var dto = new UpdateProductDto(invalidId, "Name", "Desc", 10m, 1);
        var request = new UpdateProductRequest(invalidId, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Id must be greater than or equal to 1.");
    }

    [Fact]
    public void Should_Have_Error_When_Product_Is_Null()
    {
        // Arrange
        var request = new UpdateProductRequest(1, null!);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product)
              .WithErrorMessage("Product data cannot be null.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Name_Is_Null_Or_Empty(string? invalidName)
    {
        // Arrange
        var dto = new UpdateProductDto(1, invalidName!, "Desc", 10m, 1);
        var request = new UpdateProductRequest(1, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Name)
              .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_100_Characters()
    {
        // Arrange
        var dto = new UpdateProductDto(1, new string('A', 101), "Desc", 10m, 1);
        var request = new UpdateProductRequest(1, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Name)
              .WithErrorMessage("Name cannot exceed 100 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_Value_Is_Null()
    {
        // Arrange 
        var dto = new UpdateProductDto(1, "Name", "Desc", null, 1);
        var request = new UpdateProductRequest(1, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Value)
              .WithErrorMessage("Value is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Value_Is_Negative()
    {
        // Arrange
        var dto = new UpdateProductDto(1, "Name", "Desc", -10m, 1);
        var request = new UpdateProductRequest(1, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Value)
              .WithErrorMessage("Value must be greater than or equal to 0.");
    }

    [Theory]
    [InlineData(100.123)]
    [InlineData(1.9999)]
    public void Should_Have_Error_When_Value_Has_Invalid_Decimal_Places(decimal invalidValue)
    {
        // Arrange
        var dto = new UpdateProductDto(1, "Name", "Desc", invalidValue, 1);
        var request = new UpdateProductRequest(1, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Value)
              .WithErrorMessage("Value must be a decimal with max 2 digits after the decimal point.");
    }

    [Fact]
    public void Should_Have_Error_When_Description_Exceeds_100_Characters()
    {
        // Arrange
        var dto = new UpdateProductDto(1, "Name", new string('B', 101), 10m, 1);
        var request = new UpdateProductRequest(1, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Description)
              .WithErrorMessage("Description cannot exceed 100 characters.");
    }
}