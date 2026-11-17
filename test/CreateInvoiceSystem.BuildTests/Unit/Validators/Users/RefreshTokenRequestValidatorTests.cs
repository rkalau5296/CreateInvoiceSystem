using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.RefreshToken;
using CreateInvoiceSystem.Modules.Users.Application.Validators;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Users;

public class RefreshTokenRequestValidatorTests
{
    private readonly RefreshTokenRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_RefreshToken_Is_Empty()
    {
        var request = new RefreshTokenRequest(Guid.Empty);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage("Token odświeżania jest wymagany.");
    }

    [Fact]
    public void Should_Not_Have_Errors_When_RefreshToken_Is_Valid()
    {
        var request = new RefreshTokenRequest(Guid.NewGuid());

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}