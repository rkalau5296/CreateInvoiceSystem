using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Products;

public class GetProductsRequestValidatorTests
{
    private readonly GetProductsRequestValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void Should_NotHaveValidationError_WhenPageNumberIsValid(int validPageNumber)
    {
        // Arrange
        var request = new GetProductsRequest { PageNumber = validPageNumber };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_HaveValidationError_WhenPageNumberIsLessThanOne(int invalidPageNumber)
    {
        // Arrange
        var request = new GetProductsRequest { PageNumber = invalidPageNumber };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageNumber)
            .WithErrorMessage("Page number must be greater than or equal to 1.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Should_NotHaveValidationError_WhenPageSizeIsValid(int validPageSize)
    {
        // Arrange
        var request = new GetProductsRequest { PageSize = validPageSize };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void Should_HaveValidationError_WhenPageSizeIsOutOfRange(int invalidPageSize)
    {
        // Arrange
        var request = new GetProductsRequest { PageSize = invalidPageSize };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Page size must be between 1 and 1000.");
    }

    [Fact]
    public void Should_HaveValidationError_WhenSearchTermExceeds100Characters()
    {
        // Arrange
        var longSearchTerm = new string('a', 101);
        var request = new GetProductsRequest { SearchTerm = longSearchTerm };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SearchTerm)
            .WithErrorMessage("Search term cannot exceed 100 characters.");
    }

    [Fact]
    public void Should_NotHaveValidationError_WhenRequestIsValid()
    {
        // Arrange
        var request = new GetProductsRequest
        {
            PageNumber = 1,
            PageSize = 20,
            SearchTerm = "Usługa",
            UserId = 10
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}