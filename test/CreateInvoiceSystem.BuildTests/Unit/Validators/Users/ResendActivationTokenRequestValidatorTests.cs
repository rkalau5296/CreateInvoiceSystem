using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResendToken;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Users;

public class ResendActivationTokenRequestValidatorTests
{
    private readonly ResendActivationTokenRequestValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Email_Is_Empty(string? invalidEmail)
    {
        // Arrange
        var request = new ResendActivationTokenRequest { Email = invalidEmail! };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email jest wymagany.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user@domain")]
    public void Should_Have_Error_When_Email_Format_Is_Invalid(string invalidEmail)
    {
        // Arrange
        var request = new ResendActivationTokenRequest { Email = invalidEmail };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Nieprawidłowy format adresu email.");
    }

    [Theory]
    [InlineData("jan.kowalski@example.com")]
    [InlineData("test.user+123@domain.co.uk")]
    [InlineData("admin@subdomain.domain.org")]
    public void Should_Not_Have_Errors_When_Email_Is_Valid(string validEmail)
    {
        // Arrange
        var request = new ResendActivationTokenRequest { Email = validEmail };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}