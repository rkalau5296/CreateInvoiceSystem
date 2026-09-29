using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.DeleteInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Validators;
using FluentValidation.TestHelper;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Invoices;

public class DeleteInvoiceRequestValidatorTests
{
    private readonly DeleteInvoiceRequestValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Should_Have_Error_When_Id_Is_Invalid(int invalidId)
    {
        var request = new DeleteInvoiceRequest(invalidId)
        {
            UserId = 1
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Invoice Id must be greater than 0.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Should_Have_Error_When_UserId_Is_Invalid(int invalidUserId)
    {
        var request = new DeleteInvoiceRequest(1)
        {
            UserId = invalidUserId
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User Id must be greater than 0.");
    }

    [Fact]
    public async Task Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        var request = new DeleteInvoiceRequest(10)
        {
            UserId = 1
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}