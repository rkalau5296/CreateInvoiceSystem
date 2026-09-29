using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ForgotPassword;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Users;

public class ForgotPasswordRequestValidatorTests
{
    private readonly ForgotPasswordRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Dto_Is_Null()
    {
        var request = new ForgotPasswordRequest(null!);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Dto)
            .WithErrorMessage("Dane żądania są wymagane.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Email_Is_Empty(string? invalidEmail)
    {
        var dto = new ForgotPasswordDto(invalidEmail!);
        var request = new ForgotPasswordRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Email)
            .WithErrorMessage("Adres e-mail jest wymagany.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    public void Should_Have_Error_When_Email_Format_Is_Invalid(string invalidEmail)
    {
        var dto = new ForgotPasswordDto(invalidEmail);
        var request = new ForgotPasswordRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Dto.Email)
            .WithErrorMessage("Podano nieprawidłowy format adresu e-mail.");
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        var dto = new ForgotPasswordDto("user@example.com");
        var request = new ForgotPasswordRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}