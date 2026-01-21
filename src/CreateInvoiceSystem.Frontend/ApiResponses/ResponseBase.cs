using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Frontend.ApiResponses
{
    public class ResponseBase<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }
}
