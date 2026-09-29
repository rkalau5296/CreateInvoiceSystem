using FluentValidation.TestHelper;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.CreateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using System.Runtime.CompilerServices;
using Xunit;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Invoices;

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
        var dto = CreateValidDto() with { Title = "" };
        var request = new CreateInvoiceRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Invoice.Title)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Neither_ClientId_Nor_Client_Is_Provided()
    {
        var dto = CreateValidDto() with { ClientId = null, Client = null! };
        var request = new CreateInvoiceRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Invoice)
            .WithErrorMessage("Invoice must contain a valid ClientId or Client details.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Only_ClientId_Is_Provided()
    {
        var dto = CreateValidDto() with { ClientId = 5, Client = null! };
        var request = new CreateInvoiceRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Invoice);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Only_New_Client_Is_Provided()
    {
        var dto = CreateValidDto() with
        {
            ClientId = null,
            Client = CreateValidClientDto()
        };

        var request = new CreateInvoiceRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Invoice);
    }

    [Fact]
    public void Should_Have_Error_When_PaymentDate_Is_Earlier_Than_CreatedDate()
    {
        var baseDate = DateTime.UtcNow;
        var dto = CreateValidDto() with
        {
            CreatedDate = baseDate,
            PaymentDate = baseDate.AddDays(-1)
        };
        var request = new CreateInvoiceRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Invoice.PaymentDate)
            .WithErrorMessage("PaymentDate cannot be earlier than CreatedDate.");
    }

    [Fact]
    public void Should_Have_Error_When_CreatedDate_Is_In_Future()
    {
        var polandNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Central European Standard Time");
        var dto = CreateValidDto() with { CreatedDate = polandNow.Date.AddDays(1) };
        var request = new CreateInvoiceRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Invoice.CreatedDate)
            .WithErrorMessage("CreatedDate cannot be in the future.");
    }

    [Fact]
    public void Should_Have_Error_When_TotalGross_Has_More_Than_Two_Decimal_Places()
    {
        var dto = CreateValidDto() with { TotalGross = 100.123m };
        var request = new CreateInvoiceRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Invoice.TotalGross)
            .WithErrorMessage("Value must be a decimal with max 2 digits after the decimal point.");
    }

    [Fact]
    public void Should_Have_Error_When_MethodOfPayment_Is_Too_Long()
    {
        var dto = CreateValidDto() with { MethodOfPayment = "ThisStringIsWayTooLong" };
        var request = new CreateInvoiceRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Invoice.MethodOfPayment)
            .WithErrorMessage("MethodOfPayment can have maximum 10 characters.");
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        var dto = CreateValidDto();
        var request = new CreateInvoiceRequest(dto);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static CreateInvoiceDto CreateValidDto()
    {
        return new CreateInvoiceDto
        {
            Title = "Invoice 2026/01",
            Comments = "Standard invoice",
            MethodOfPayment = "Transfer",
            TotalNet = 1016.26m,
            TotalVat = 233.74m,
            TotalGross = 1250.00m,
            PaymentDate = DateTime.UtcNow.AddDays(14),
            CreatedDate = DateTime.UtcNow.AddMinutes(-5),
            UserId = 1,
            ClientId = 5,
            Client = null!,
            InvoicePositions = new List<InvoicePositionDto>
            {
                new InvoicePositionDto(0, 0, 1, null!, "Item", "Desc", 1250.00m, 1, "23%")
            }
        };
    }

    private static dynamic CreateValidClientDto()
    {
        var clientType = typeof(CreateInvoiceDto).GetProperty(nameof(CreateInvoiceDto.Client))!.PropertyType;
        var clientInstance = RuntimeHelpers.GetUninitializedObject(clientType);

        var nameProp = clientType.GetProperty("Name") ?? clientType.GetProperty("ClientName");
        if (nameProp != null && nameProp.CanWrite)
        {
            nameProp.SetValue(clientInstance, "Test Client");
        }

        return clientInstance!;
    }
}