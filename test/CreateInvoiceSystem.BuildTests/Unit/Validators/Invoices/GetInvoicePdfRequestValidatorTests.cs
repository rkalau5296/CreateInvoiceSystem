using CreateInvoiceSystem.Modules.Invoices.Application.Validators;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetPdf;
using FluentValidation.TestHelper;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Invoices;

public class GetInvoicePdfRequestValidatorTests
{
    private readonly GetInvoicePdfRequestValidator _validator = new();

    [Theory]
    [InlineData(1, 1)]
    [InlineData(100, 50)]
    public void Should_NotHaveValidationErrors_WhenRequestIsValid(int validInvoiceId, int validUserId)
    {
        // Arrange
        var request = new GetInvoicePdfRequest(validInvoiceId, validUserId);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_HaveValidationError_WhenInvoiceIdIsLessThanOne(int invalidInvoiceId)
    {
        // Arrange
        var request = new GetInvoicePdfRequest(invalidInvoiceId, 1);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InvoiceId)
            .WithErrorMessage("Invoice ID must be greater than or equal to 1.");
    }    
}