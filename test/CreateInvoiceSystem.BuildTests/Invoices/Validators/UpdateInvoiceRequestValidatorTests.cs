using FluentValidation.TestHelper;
using Xunit;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.UpdateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;

namespace CreateInvoiceSystem.BuildTests.Invoices.Validators;

public class UpdateInvoiceRequestValidatorTests
{
    private readonly UpdateInvoiceRequestValidator _validator;

    public UpdateInvoiceRequestValidatorTests()
    {
        _validator = new UpdateInvoiceRequestValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        // Arrange
        var dto = CreateValidDto() with { Title = "" };
        var request = new UpdateInvoiceRequest(dto.InvoiceId, dto);

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
        var request = new UpdateInvoiceRequest(dto.InvoiceId, dto);

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
        var baseDate = DateTime.Now;
        var dto = CreateValidDto() with
        {
            CreatedDate = baseDate,
            PaymentDate = baseDate.AddDays(-1)
        };
        var request = new UpdateInvoiceRequest(dto.InvoiceId, dto);

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
        var dto = CreateValidDto() with { CreatedDate = DateTime.Now.AddDays(1) };
        var request = new UpdateInvoiceRequest(dto.InvoiceId, dto);

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
        var dto = CreateValidDto() with { TotalAmount = 500.1234m };
        var request = new UpdateInvoiceRequest(dto.InvoiceId, dto);

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
        var dto = CreateValidDto() with { MethodOfPayment = "LongerThanTenChars" };
        var request = new UpdateInvoiceRequest(dto.InvoiceId, dto);

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
        var request = new UpdateInvoiceRequest(dto.InvoiceId, dto);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    private UpdateInvoiceDto CreateValidDto()
    {
        return new UpdateInvoiceDto(
            InvoiceId: 1,
            Title: "FV/2026/01/01",
            TotalAmount: 1000.00m,
            PaymentDate: DateTime.Now.AddDays(14),
            CreatedDate: DateTime.Now.AddHours(-1),
            Comments: "Test comments",
            ClientId: 10,
            UserId: 1,
            Client: null,
            MethodOfPayment: "Transfer",
            InvoicePositions: new List<UpdateInvoicePositionDto>(),
            ClientName: "Client Name",
            ClientNip: "1234567890",
            ClientAddress: "Street 1, City"
        );
    }
}