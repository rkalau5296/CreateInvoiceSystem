using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ActivateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Users;

public class ActivateUserRequestValidatorTests
{
    private readonly ActivateUserRequestValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Token_Is_Null_Or_Whitespace(string? invalidToken)
    {
        var request = new ActivateUserRequest { Token = invalidToken! };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage("Błąd: Brak tokena aktywacyjnego.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Token_Is_Provided()
    {
        var request = new ActivateUserRequest { Token = "some.valid.jwt_token" };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }
}