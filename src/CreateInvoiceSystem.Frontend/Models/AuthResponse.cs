using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Frontend.Models
{
    public class AuthResponse
    {
        [JsonPropertyName("token")] 
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}
