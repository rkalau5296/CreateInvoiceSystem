using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Clients;

public class GetClientsRequestValidatorTests
{
    private readonly GetClientsRequestValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void Should_NotHaveValidationError_WhenPageNumberIsValid(int validPage)
    {
        // Arrange
        var request = new GetClientsRequest { PageNumber = validPage };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_HaveValidationError_WhenPageNumberIsInvalid(int invalidPage)
    {
        // Arrange
        var request = new GetClientsRequest { PageNumber = invalidPage };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Should_HaveValidationError_WhenPageSizeIsOutOfRange(int invalidSize)
    {
        // Arrange
        var request = new GetClientsRequest { PageSize = invalidSize };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}