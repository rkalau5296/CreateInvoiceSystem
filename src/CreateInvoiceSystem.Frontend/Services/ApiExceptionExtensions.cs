using System.Text.Json;

namespace CreateInvoiceSystem.Frontend.Services
{
    public static class ApiExceptionExtensions
    {
        public static string GetUserMessage(this ApiException ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Response))
            {
                try
                {
                    using var doc = JsonDocument.Parse(ex.Response);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("detail", out var d) && d.ValueKind == JsonValueKind.String)
                        return d.GetString() ?? ex.Message;

                    if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
                        return t.GetString() ?? ex.Message;

                    if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in errors.EnumerateObject())
                        {
                            var arr = prop.Value;
                            if (arr.ValueKind == JsonValueKind.Array && arr.GetArrayLength() > 0)
                            {
                                var first = arr[0].GetString();
                                if (!string.IsNullOrWhiteSpace(first))
                                    return first;
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            return ex.StatusCode switch
            {
                400 => "Nieprawidłowe dane (sprawdź parametry, np. zakres dat).",
                404 => "Brak danych dla podanych parametrów.",
                409 => "Konflikt danych (konflikt z bazą).",
                401 => "Brak autoryzacji — zaloguj się ponownie.",
                403 => "Brak uprawnień do wykonania tej operacji.",
                _ => ex.Message ?? "Wystąpił błąd po stronie serwera."
            };
        }
    }
}