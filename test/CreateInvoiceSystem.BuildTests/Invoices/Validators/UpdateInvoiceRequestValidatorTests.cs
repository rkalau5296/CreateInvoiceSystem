using FluentValidation.TestHelper;
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
        var dto = CreateValidDto() with { Title = "" };
        var request = new UpdateInvoiceRequest(dto.InvoiceId, dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Invoice.Title)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_TotalNet_Has_More_Than_Two_Decimal_Places()
    {
        var dto = CreateValidDto() with { TotalNet = 500.1234m };
        var request = new UpdateInvoiceRequest(dto.InvoiceId, dto);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Invoice.TotalNet)
            .WithErrorMessage("Value must be a decimal with max 2 digits after the decimal point.");
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Request_Is_Valid()
    {
        var dto = CreateValidDto();
        var request = new UpdateInvoiceRequest(dto.InvoiceId, dto);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private UpdateInvoiceDto CreateValidDto()
    {      
        return new UpdateInvoiceDto(
            InvoiceId: 1,
            Title: "FV/2026/01/01",
            TotalNet: 1000.00m,
            TotalVat: 230.00m,
            TotalGross: 1230.00m,
            PaymentDate: DateTime.Now.AddDays(14),
            CreatedDate: DateTime.Now.AddHours(-1),
            Comments: "Test comments",
            ClientId: 10,
            UserId: 1,
            Client: null,
            MethodOfPayment: "Transfer",
            InvoicePositions: new List<UpdateInvoicePositionDto>(),
            SellerName: "My Company",
            SellerNip: "9876543210",
            SellerAddress: "Warszawa",
            BankAccountNumber: "PL123456789",
            ClientName: "Client Name",
            ClientNip: "1234567890",
            ClientAddress: "Street 1, City"
        );
    }
}