using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.DeleteClient;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Validators;
using FluentValidation.TestHelper;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Clients;

public class DeleteClientRequestValidatorTests
{
    private readonly DeleteClientRequestValidator _validator;

    public DeleteClientRequestValidatorTests()
    {
        _validator = new DeleteClientRequestValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_LessThanOne()
    {
        // Arrange
        var request = new DeleteClientRequest(0);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Id_Is_Valid()
    {
        // Arrange
        var request = new DeleteClientRequest(1);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}