using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Frontend.Models;

public sealed class KsefInvoiceDto
{
    [JsonPropertyName("ksefNumber")]
    public string KsefNumber { get; set; } = string.Empty;

    [JsonPropertyName("invoiceNumber")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [JsonPropertyName("issueDate")]
    public DateTimeOffset IssueDate { get; set; }

    [JsonPropertyName("sellerName")]
    public string? SellerName { get; set; }

    [JsonPropertyName("sellerNip")]
    public string? SellerNip { get; set; }

    [JsonPropertyName("buyerName")]
    public string? BuyerName { get; set; }

    [JsonPropertyName("netAmount")]
    public decimal NetAmount { get; set; }

    [JsonPropertyName("vatAmount")]
    public decimal VatAmount { get; set; }

    [JsonPropertyName("grossAmount")]
    public decimal GrossAmount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
}
