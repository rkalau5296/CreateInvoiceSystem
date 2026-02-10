using CreateInvoiceSystem.Frontend.Models;
using System.Net.Http.Json;

namespace CreateInvoiceSystem.Frontend.Services
{
    public class UserService
    {
        private readonly HttpClient _http;

        public UserService(HttpClient http) => _http = http;

        public async Task<GetUserResponse?> GetMe()
            => await _http.GetFromJsonAsync<GetUserResponse>("api/User/me");

        public async Task UpdateUser(int id, UpdateUserDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/User/update/{id}", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error: {error}");
            }
        }
    }
}