using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.CreateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using FluentValidation.TestHelper;

namespace CreateInvoiceSystem.BuildTests.Products.Validators;

public class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator;

    public CreateProductRequestValidatorTests()
    {
        _validator = new CreateProductRequestValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var dto = new CreateProductDto("", "Description", 10.00m, 1);
        var request = new CreateProductRequest(dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Name)
              .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_100_Characters()
    {
        // Arrange
        var dto = new CreateProductDto(new string('A', 101), "Description", 10.00m, 1);
        var request = new CreateProductRequest(dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Name);
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Zero_Or_Less()
    {
        // Arrange
        var dto = new CreateProductDto("Test", "Description", 10.00m, 0);
        var request = new CreateProductRequest(dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.UserId)
              .WithErrorMessage("UserId is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Value_Is_Null()
    {
        // Arrange
        var dto = new CreateProductDto("Test", "Description", null, 1);
        var request = new CreateProductRequest(dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Value)
              .WithErrorMessage("Value is required.");
    }

    [Theory]
    [InlineData(10.555)] 
    [InlineData(10.1234)]
    public void Should_Have_Error_When_Value_Has_More_Than_Two_Decimal_Places(decimal invalidValue)
    {
        // Arrange
        var dto = new CreateProductDto("Test", "Description", invalidValue, 1);
        var request = new CreateProductRequest(dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Product.Value)
              .WithErrorMessage("Value must be a decimal with max 2 digits after the decimal point.");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(10.5)]
    [InlineData(10.55)]
    public void Should_Not_Have_Error_When_Value_Is_Correct(decimal validValue)
    {
        // Arrange
        var dto = new CreateProductDto("Test", "Description", validValue, 1);
        var request = new CreateProductRequest(dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Product.Value);
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        // Arrange
        var dto = new CreateProductDto("Valid Product", "Good Description", 99.99m, 1);
        var request = new CreateProductRequest(dto);

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}