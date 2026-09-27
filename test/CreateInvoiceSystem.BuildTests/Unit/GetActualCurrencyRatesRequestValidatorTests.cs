using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRates;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class GetActualCurrencyRatesRequestValidatorTests
{
    private readonly GetActualCurrencyRatesRequestValidator _validator = new();

    [Theory]
    [InlineData("A")]
    [InlineData("b")]
    [InlineData("C")]
    public void Should_NotHaveValidationErrors_WhenTableNameIsValid(string tableName)
    {
        // Arrange
        var request = new GetActualCurrencyRatesRequest(tableName);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_HaveValidationError_WhenTableNameIsEmpty(string? invalidTable)
    {
        // Arrange
        var request = new GetActualCurrencyRatesRequest(invalidTable!);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TableName)
            .WithErrorMessage("Table name is required.");
    }

    [Theory]
    [InlineData("D")]
    [InlineData("X")]
    [InlineData("123")]
    public void Should_HaveValidationError_WhenTableNameIsInvalid(string invalidTable)
    {
        // Arrange
        var request = new GetActualCurrencyRatesRequest(invalidTable);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TableName)
            .WithErrorMessage("Table name must be 'A', 'B', or 'C'.");
    }
}