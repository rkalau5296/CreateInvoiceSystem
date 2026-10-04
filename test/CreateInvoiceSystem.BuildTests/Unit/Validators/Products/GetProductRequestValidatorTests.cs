using CreateInvoiceSystem.Modules.Products.Application.Validators;
using CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.GetProduct;
using FluentValidation.TestHelper;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Products;

public class GetProductRequestValidatorTests
{
    private readonly GetProductRequestValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Should_NotHaveValidationError_WhenIdIsValid(int validId)
    {
        // Arrange
        var request = new GetProductRequest(validId);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_HaveValidationError_WhenIdIsLessThanOne(int invalidId)
    {
        // Arrange
        var request = new GetProductRequest(invalidId);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Id must be greater than or equal to 1.");
    }
}