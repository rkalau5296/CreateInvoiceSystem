using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.GetUser;
using CreateInvoiceSystem.Modules.Users.Application.Validators;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Users;

public class GetUserRequestValidatorTests
{
    private readonly GetUserRequestValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Have_Error_When_Id_Is_Less_Than_One(int invalidId)
    {
        var request = new GetUserRequest(invalidId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Id must be greater than or equal to 1.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Should_Not_Have_Error_When_Id_Is_Valid(int validId)
    {
        var request = new GetUserRequest(validId);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}