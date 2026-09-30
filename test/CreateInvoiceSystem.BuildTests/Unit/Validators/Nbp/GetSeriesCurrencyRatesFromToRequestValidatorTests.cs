using System;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.PreviousDatesRates;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Nbp;

public class GetSeriesCurrencyRatesFromToRequestValidatorTests
{
    private readonly GetSeriesCurrencyRatesFromToRequestValidator _validator = new();

    [Fact]
    public void Should_NotHaveValidationErrors_WhenRequestIsValid()
    {
        // Arrange
        var dateFrom = DateTime.Today.AddDays(-10);
        var dateTo = DateTime.Today.AddDays(-1);
        var request = new GetSeriesCurrencyRatesFromToRequest("A", dateFrom, dateTo);

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
        var request = new GetSeriesCurrencyRatesFromToRequest(invalidTable!, DateTime.Today.AddDays(-5), DateTime.Today);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TableName)
            .WithErrorMessage("Table name is required.");
    }

    [Theory]
    [InlineData("D")]
    [InlineData("X")]
    public void Should_HaveValidationError_WhenTableNameIsInvalid(string invalidTable)
    {
        // Arrange
        var request = new GetSeriesCurrencyRatesFromToRequest(invalidTable, DateTime.Today.AddDays(-5), DateTime.Today);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TableName)
            .WithErrorMessage("Table name must be 'A', 'B', or 'C'.");
    }

    [Fact]
    public void Should_HaveValidationError_WhenDateFromIsAfterDateTo()
    {
        // Arrange
        var dateFrom = DateTime.Today;
        var dateTo = DateTime.Today.AddDays(-5);
        var request = new GetSeriesCurrencyRatesFromToRequest("A", dateFrom, dateTo);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateFrom)
            .WithErrorMessage("DateFrom must be less than or equal to DateTo.");
    }

    [Fact]
    public void Should_HaveValidationError_WhenDateToIsInTheFuture()
    {
        // Arrange
        var dateFrom = DateTime.Today;
        var dateTo = DateTime.Today.AddDays(1);
        var request = new GetSeriesCurrencyRatesFromToRequest("A", dateFrom, dateTo);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateTo)
            .WithErrorMessage("DateTo cannot be in the future.");
    }
}