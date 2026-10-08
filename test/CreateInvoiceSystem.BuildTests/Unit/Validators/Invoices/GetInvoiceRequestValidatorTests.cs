using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetInvoice;
using CreateInvoiceSystem.Modules.Invoices.Application.Validators;
using FluentValidation.TestHelper;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Invoices;

public class GetInvoiceRequestValidatorTests
{
    private readonly GetInvoiceRequestValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Should_Have_Error_When_Id_Is_Less_Than_One(
        int invalidId)
    {
        var request = new GetInvoiceRequest(invalidId);

        var result = await _validator.TestValidateAsync(
            request,
            options: null,
            cancellationToken: TestContext.Current.CancellationToken);

        result.ShouldHaveValidationErrorFor(
                requestItem => requestItem.Id)
            .WithErrorMessage(
                "Id must be greater than or equal to 1.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Should_Not_Have_Error_When_Id_Is_Valid(
        int validId)
    {
        var request = new GetInvoiceRequest(validId);

        var result = await _validator.TestValidateAsync(
            request,
            options: null,
            cancellationToken: TestContext.Current.CancellationToken);

        result.ShouldNotHaveValidationErrorFor(
            requestItem => requestItem.Id);
    }
}