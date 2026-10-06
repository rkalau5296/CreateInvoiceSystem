using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.DeleteUser;
using CreateInvoiceSystem.Modules.Users.Application.Validators;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Users;

public class DeleteUserRequestValidatorTests
{
    private readonly DeleteUserRequestValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Have_Error_When_Id_Is_Less_Than_One(int invalidId)
    {
        var request = new DeleteUserRequest(invalidId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Should_Not_Have_Error_When_Id_Is_Valid(int validId)
    {
        var request = new DeleteUserRequest(validId);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}