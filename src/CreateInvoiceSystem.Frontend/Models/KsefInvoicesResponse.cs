using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Frontend.Models;

public sealed class KsefInvoicesResponse
{
    [JsonPropertyName("data")]
    public List<KsefInvoiceDto> Data { get; set; } = new();

    [JsonPropertyName("hasMore")]
    public bool HasMore { get; set; }
}
