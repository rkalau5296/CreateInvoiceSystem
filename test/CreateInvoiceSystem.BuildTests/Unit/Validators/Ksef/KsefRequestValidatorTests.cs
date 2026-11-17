using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetIncomingKsefInvoices;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceDetails;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoicePdf;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceStatus;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceXml;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.KsefSettings;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.SendInvoiceToKsef;
using CreateInvoiceSystem.Modules.Ksef.Application.Validators;
using FluentValidation;

namespace CreateInvoiceSystem.BuildTests.Unit.Validators.Ksef;

public class KsefRequestValidatorTests
{
    [Theory]
    [InlineData(0, 20, false)]
    [InlineData(-1, 20, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, 101, false)]
    [InlineData(1, 1, true)]
    [InlineData(1, 100, true)]
    [InlineData(21474837, 100, true)]
    [InlineData(21474838, 100, false)]
    public void IncomingInvoices_ValidatesPagination(int page, int size, bool valid)
    {
        var request = new GetIncomingKsefInvoicesRequest { UserId = 1, PageNumber = page, PageSize = size };
        Assert.Equal(valid, new GetIncomingKsefInvoicesRequestValidator().Validate(request).IsValid);
    }

    [Fact]
    public void IncomingInvoices_ValidatesDateOrderAndUser()
    {
        var validator = new GetIncomingKsefInvoicesRequestValidator();
        var request = new GetIncomingKsefInvoicesRequest
        {
            UserId = 0, FromDate = new DateOnly(2026, 2, 1), ToDate = new DateOnly(2026, 1, 1)
        };
        var result = validator.Validate(request);
        Assert.Contains(result.Errors, error => error.PropertyName == "UserId");
        Assert.Contains(result.Errors, error => error.PropertyName == "FromDate");
        request.UserId = 1;
        request.ToDate = request.FromDate;
        Assert.True(validator.Validate(request).IsValid);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("  ", false)]
    [InlineData("token with space", false)]
    [InlineData("token\tvalue", false)]
    [InlineData("token-value", true)]
    public void Settings_ValidatesToken(string? token, bool valid)
    {
        var result = new SaveKsefSettingsRequestValidator().Validate(new SaveKsefSettingsRequest(1, token!));
        Assert.Equal(valid, result.IsValid);
    }

    [Fact]
    public void Settings_ValidatesTokenLength()
    {
        var validator = new SaveKsefSettingsRequestValidator();
        Assert.True(validator.Validate(new SaveKsefSettingsRequest(1, new string('a', 4096))).IsValid);
        Assert.False(validator.Validate(new SaveKsefSettingsRequest(1, new string('a', 4097))).IsValid);
    }

    [Fact]
    public void AllRequests_RequirePositiveUserId()
    {
        AssertInvalidUser(new GetIncomingKsefInvoicesRequestValidator(), new GetIncomingKsefInvoicesRequest());
        AssertInvalidUser(new GetKsefInvoiceDetailsRequestValidator(), new GetKsefInvoiceDetailsRequest("number"));
        AssertInvalidUser(new GetKsefInvoicePdfRequestValidator(), new GetKsefInvoicePdfRequest(0, "number"));
        AssertInvalidUser(new GetKsefInvoiceXmlRequestValidator(), new GetKsefInvoiceXmlRequest("number"));
        AssertInvalidUser(new GetKsefInvoiceStatusRequestValidator(), new GetKsefInvoiceStatusRequest(1));
        AssertInvalidUser(new SendInvoiceToKsefRequestValidator(), new SendInvoiceToKsefRequest(1));
        AssertInvalidUser(new GetKsefSettingsRequestValidator(), new GetKsefSettingsRequest(0));
        AssertInvalidUser(new DeleteKsefSettingsRequestValidator(), new DeleteKsefSettingsRequest(0));
        AssertInvalidUser(new SaveKsefSettingsRequestValidator(), new SaveKsefSettingsRequest(0, "token"));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    public void SendAndStatus_ValidateInvoiceId(int invoiceId, bool valid)
    {
        Assert.Equal(valid, new SendInvoiceToKsefRequestValidator().Validate(new SendInvoiceToKsefRequest(invoiceId) { UserId = 1 }).IsValid);
        Assert.Equal(valid, new GetKsefInvoiceStatusRequestValidator().Validate(new GetKsefInvoiceStatusRequest(invoiceId) { UserId = 1 }).IsValid);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("  ", false)]
    [InlineData("number", true)]
    public void InvoiceDownloads_ValidateKsefNumber(string number, bool valid)
    {
        Assert.Equal(valid, new GetKsefInvoiceDetailsRequestValidator().Validate(new GetKsefInvoiceDetailsRequest(number) { UserId = 1 }).IsValid);
        Assert.Equal(valid, new GetKsefInvoiceXmlRequestValidator().Validate(new GetKsefInvoiceXmlRequest(number) { UserId = 1 }).IsValid);
        Assert.Equal(valid, new GetKsefInvoicePdfRequestValidator().Validate(new GetKsefInvoicePdfRequest(1, number)).IsValid);
    }

    private static void AssertInvalidUser<T>(IValidator<T> validator, T request)
    {
        Assert.Contains(validator.Validate(request).Errors, error => error.PropertyName == "UserId");
    }
}
