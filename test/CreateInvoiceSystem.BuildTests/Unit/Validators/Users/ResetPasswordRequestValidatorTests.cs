using FluentValidation.TestHelper;
using Xunit;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ResetPassword;
using CreateInvoiceSystem.Modules.Users.Application.Validators;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Users;

public class ResetPasswordRequestValidatorTests
{
    private readonly ResetPasswordRequestValidator _validator = new();

    private static ResetPasswordRequest CreateValidRequest() => new()
    {
        Email = "jan.kowalski@example.com",
        Token = "valid-reset-token-123",
        Version = "1",
        NewPassword = "NewSecurePassword123!"
    };

    [Theory]
    [InlineData(null, "Email jest wymagany.")]
    [InlineData("", "Email jest wymagany.")]
    [InlineData("   ", "Email jest wymagany.")]
    [InlineData("invalid-email", "Nieprawidłowy format adresu email.")]
    [InlineData("user@", "Nieprawidłowy format adresu email.")]
    [InlineData("@domain.com", "Nieprawidłowy format adresu email.")]
    public void Should_Have_Error_When_Email_Is_Invalid(string? email, string expectedMessage)
    {
        var request = CreateValidRequest();
        request.Email = email!;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(expectedMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Token_Is_Empty(string? token)
    {
        var request = CreateValidRequest();
        request.Token = token!;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage("Token jest wymagany.");
    }

    [Theory]
    [InlineData(null, "Wersja jest wymagana.")]
    [InlineData("", "Wersja jest wymagana.")]
    [InlineData("   ", "Wersja jest wymagana.")]
    [InlineData("abc", "Wersja musi być poprawną liczbą większą lub równą 0.")]
    [InlineData("-1", "Wersja musi być poprawną liczbą większą lub równą 0.")]
    public void Should_Have_Error_When_Version_Is_Invalid(string? version, string expectedMessage)
    {
        var request = CreateValidRequest();
        request.Version = version!;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Version)
            .WithErrorMessage(expectedMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_NewPassword_Is_Empty(string? newPassword)
    {
        var request = CreateValidRequest();
        request.NewPassword = newPassword!;

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Nowe hasło jest wymagane.");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("10")]
    public void Should_Not_Have_Errors_When_Request_Is_Valid(string validVersion)
    {
        var request = CreateValidRequest();
        request.Version = validVersion;

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}