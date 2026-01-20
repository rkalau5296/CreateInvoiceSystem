using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.CreateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;

namespace CreateInvoiceSystem.BuildTests.Invoices.Validators;

public class CreateInvoiceRequestValidatorTests
{
    private readonly CreateInvoiceRequestValidator _validator;

    public CreateInvoiceRequestValidatorTests()
    {
        _validator = new CreateInvoiceRequestValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        // Arrange
        var dto = CreateValidDto() with { Title = "" };
        var request = new CreateInvoiceRequest(dto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Invoice.Title)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_ClientId_Is_Zero_Or_Less()
    {
        // Arrange
        var dto = CreateValidDto() with { ClientId = 0 };
        var request = new CreateInvoiceRequest(dto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Invoice.ClientId)
            .WithErrorMessage("ClientId is required.");
    }

    [Fact]
    public void Should_Have_Error_When_PaymentDate_Is_Earlier_Than_CreatedDate()
    {
        // Arrange
        var baseDate = DateTime.UtcNow;
        var dto = CreateValidDto() with
        {
            CreatedDate = baseDate,
            PaymentDate = baseDate.AddDays(-1)
        };
        var request = new CreateInvoiceRequest(dto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Invoice.PaymentDate)
            .WithErrorMessage("PaymentDate cannot be earlier than CreatedDate.");
    }

    [Fact]
    public void Should_Have_Error_When_CreatedDate_Is_In_Future()
    {
        // Arrange
        var dto = CreateValidDto() with { CreatedDate = DateTime.UtcNow.AddDays(1) };
        var request = new CreateInvoiceRequest(dto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Invoice.CreatedDate)
            .WithErrorMessage("CreatedDate cannot be in the future.");
    }

    [Fact]
    public void Should_Have_Error_When_TotalAmount_Has_More_Than_Two_Decimal_Places()
    {
        // Arrange
        var dto = CreateValidDto() with { TotalAmount = 100.123m };
        var request = new CreateInvoiceRequest(dto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Invoice.TotalAmount)
            .WithErrorMessage("Value must be a decimal with max 2 digits after the decimal point.");
    }

    [Fact]
    public void Should_Have_Error_When_MethodOfPayment_Is_Too_Long()
    {
        // Arrange
        var dto = CreateValidDto() with { MethodOfPayment = "ThisStringIsWayTooLong" };
        var request = new CreateInvoiceRequest(dto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Invoice.MethodOfPayment)
            .WithErrorMessage("MethodOfPayment can have maximum 10 characters.");
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        // Arrange
        var dto = CreateValidDto();
        var request = new CreateInvoiceRequest(dto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    private CreateInvoiceDto CreateValidDto()
    {
        return new CreateInvoiceDto
        {
            Title = "Invoice 2026/01",
            Comments = "Standard invoice",
            MethodOfPayment = "Transfer",
            TotalAmount = 1250.00m,
            PaymentDate = DateTime.UtcNow.AddDays(14),
            CreatedDate = DateTime.UtcNow.AddMinutes(-5),
            UserId = 1,
            ClientId = 5,
            InvoicePositions = new List<InvoicePositionDto>
            {
                new InvoicePositionDto(0, 0, 1, null, "Item", "Desc", 1250.00m, 1)
            }
        };
    }
}