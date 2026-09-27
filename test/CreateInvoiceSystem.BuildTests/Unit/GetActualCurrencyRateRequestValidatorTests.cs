using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRate;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class GetActualCurrencyRateRequestValidatorTests
{
    private readonly GetActualCurrencyRateRequestValidator _validator = new();

    [Theory]
    [InlineData("A", "EUR")]
    [InlineData("b", "usd")]
    [InlineData("C", "CHF")]
    public void Should_NotHaveValidationErrors_WhenRequestIsValid(string tableName, string currencyCode)
    {
        // Arrange
        var request = new GetActualCurrencyRateRequest(tableName, currencyCode);

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
        var request = new GetActualCurrencyRateRequest(invalidTable!, "EUR");

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
        var request = new GetActualCurrencyRateRequest(invalidTable, "EUR");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TableName)
            .WithErrorMessage("Table name must be 'A', 'B', or 'C'.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_HaveValidationError_WhenCurrencyCodeIsEmpty(string? invalidCurrency)
    {
        // Arrange
        var request = new GetActualCurrencyRateRequest("A", invalidCurrency!);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrencyCode)
            .WithErrorMessage("Currency code is required.");
    }

    [Theory]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Should_HaveValidationError_WhenCurrencyCodeLengthIsNotThree(string invalidCurrency)
    {
        // Arrange
        var request = new GetActualCurrencyRateRequest("A", invalidCurrency);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrencyCode)
            .WithErrorMessage("Currency code must be exactly 3 characters long.");
    }

    [Theory]
    [InlineData("EU1")]
    [InlineData("E-R")]
    public void Should_HaveValidationError_WhenCurrencyCodeContainsNonLetters(string invalidCurrency)
    {
        // Arrange
        var request = new GetActualCurrencyRateRequest("A", invalidCurrency);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrencyCode)
            .WithErrorMessage("Currency code must consist of 3 letters.");
    }
}