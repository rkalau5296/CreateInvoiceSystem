using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Frontend.Models;

public sealed class KsefInvoiceDetailsDto
{
    [JsonPropertyName("ksefNumber")]
    public string KsefNumber { get; set; } = string.Empty;

    [JsonPropertyName("invoiceNumber")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [JsonPropertyName("issueDate")]
    public DateOnly? IssueDate { get; set; }

    [JsonPropertyName("saleDate")]
    public DateOnly? SaleDate { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "PLN";

    [JsonPropertyName("seller")]
    public KsefInvoicePartyDto Seller { get; set; } = new();

    [JsonPropertyName("buyer")]
    public KsefInvoicePartyDto Buyer { get; set; } = new();

    [JsonPropertyName("lines")]
    public List<KsefInvoiceLineDto> Lines { get; set; } = new();

    [JsonPropertyName("netAmount")]
    public decimal NetAmount { get; set; }

    [JsonPropertyName("vatAmount")]
    public decimal VatAmount { get; set; }

    [JsonPropertyName("grossAmount")]
    public decimal GrossAmount { get; set; }

    [JsonPropertyName("paymentDueDate")]
    public DateOnly? PaymentDueDate { get; set; }

    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("bankAccount")]
    public string? BankAccount { get; set; }
}

public sealed class KsefInvoicePartyDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("nip")]
    public string? Nip { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

public sealed class KsefInvoiceLineDto
{
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("unitOfMeasure")]
    public string? UnitOfMeasure { get; set; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("netAmount")]
    public decimal NetAmount { get; set; }

    [JsonPropertyName("vatRate")]
    public string? VatRate { get; set; }
}
