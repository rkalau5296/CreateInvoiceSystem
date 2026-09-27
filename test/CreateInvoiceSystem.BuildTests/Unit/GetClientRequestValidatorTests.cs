using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class GetClientRequestValidatorTests
{
    private readonly GetClientRequestValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Should_NotHaveValidationError_WhenIdIsGreaterThanOrEqualToOne(int validId)
    {
        // Arrange
        var request = new GetClientRequest(validId);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_HaveValidationError_WhenIdIsLessThanOne(int invalidId)
    {
        // Arrange
        var request = new GetClientRequest(invalidId);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}