using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CreateInvoiceSystem.Frontend.Services
{
    public static class HttpResponseExtensions
    {
        public static async Task EnsureSuccessOrThrowApiExceptionAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            var content = await response.Content.ReadAsStringAsync();
            string message = $"API error ({(int)response.StatusCode})";

            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                    {
                        message = detail.GetString() ?? message;
                    }
                    else if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    {
                        message = title.GetString() ?? message;
                    }
                    else if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                    {
                        message = "Wystąpiły błędy walidacji";
                    }
                    else
                    {                        
                        message = content;
                    }
                }
                catch
                {                    
                    message = content;
                }
            }

            throw new ApiException(message, (int)response.StatusCode, content);
        }
    }
}