using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.UpdateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using FluentValidation.TestHelper;

namespace CreateInvoiceSystem.BuildTests.Products.Validators;

public class UpdateProductRequestValidatorTests
{
    private readonly UpdateProductRequestValidator _validator;

    public UpdateProductRequestValidatorTests()
    {
        _validator = new UpdateProductRequestValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty_String()
    {
        // Arrange
        var dto = new UpdateProductDto(1, "", "Desc", 10m, 1, false);
        var request = new UpdateProductRequest(1, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Name)
              .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Null()
    {
        // Arrange 
        var dto = new UpdateProductDto(1, null!, "Desc", 10m, 1, false);
        var request = new UpdateProductRequest(1, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Product.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_100_Characters()
    {
        // Arrange
        var dto = new UpdateProductDto(1, new string('A', 101), "Desc", 10m, 1, false);
        var request = new UpdateProductRequest(1, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Name);
    }

    [Theory]
    [InlineData(100.123)]
    [InlineData(1.9999)]
    public void Should_Have_Error_When_Value_Has_Invalid_Decimal_Places(decimal invalidValue)
    {
        // Arrange
        var dto = new UpdateProductDto(1, "Name", "Desc", invalidValue, 1, false);
        var request = new UpdateProductRequest(1, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Value)
              .WithErrorMessage("Value must be a decimal with max 2 digits after the decimal point.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Value_Is_Null()
    {
        // Arrange 
        var dto = new UpdateProductDto(1, "Name", "Desc", null, 1, false);
        var request = new UpdateProductRequest(1, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Product.Value);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Exceeds_100_Characters()
    {
        // Arrange
        var dto = new UpdateProductDto(1, "Name", new string('B', 101), 10m, 1, false);
        var request = new UpdateProductRequest(1, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Description);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        // Arrange
        var dto = new UpdateProductDto(1, "Valid Name", "Short Desc", 49.99m, 1, false);
        var request = new UpdateProductRequest(1, dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}