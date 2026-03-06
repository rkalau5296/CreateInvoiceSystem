using System.Text.Json;

namespace CreateInvoiceSystem.Frontend.Services;

public static class ApiExceptionExtensions
{
    public static string GetUserMessage(this ApiException ex)
    {
        var msg = TryGetFirstValidationMessage(ex.Response)
               ?? TryGetMessageFromResponse(ex.Response)
               ?? (string.IsNullOrWhiteSpace(ex.Message) ? null : ex.Message)
               ?? StatusFallback(ex.StatusCode);

        return msg;
    }

    public static string SanitizeMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Wystąpił błąd po stronie serwera. Spróbuj ponownie później.";

        var internalSanitized = SanitizeInternalErrors(message);
        if (!string.IsNullOrEmpty(internalSanitized))
            return internalSanitized;

        var lower = message.ToLowerInvariant();
        if (lower.Contains("nbp api") || lower.Contains("nbpapi") || (lower.Contains("badrequest") && lower.Contains("nbp")))
            return "Błąd serwisu NBP. Sprawdź parametry.";

        return "Wystąpił błąd po stronie serwera. Spróbuj ponownie później.";
    }

    private static string? TryGetMessageFromResponse(string? responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("detail", out var d) && d.ValueKind == JsonValueKind.String)
                return d.GetString();

            if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
                return t.GetString();

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in errors.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array && prop.Value.GetArrayLength() > 0)
                    {
                        var first = prop.Value[0].GetString();
                        if (!string.IsNullOrWhiteSpace(first))
                            return first;
                    }
                    else if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        var s = prop.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(s))
                            return s;
                    }
                }
            }
        }
        catch
        {
        }

        return null;
    }

    private static string? TryGetFirstValidationMessage(string? responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in errors.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array && prop.Value.GetArrayLength() > 0)
                    {
                        var first = prop.Value[0].GetString();
                        if (!string.IsNullOrWhiteSpace(first))
                            return first;
                    }
                }
            }
        }
        catch
        {
        }

        return null;
    }

    private static string? SanitizeInternalErrors(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        var lower = message.ToLowerInvariant();

        if (lower.Contains("a network-related or instance-specific error")
            || lower.Contains("nie można nawiązać połączenia")
            || lower.Contains("actively refused")
            || lower.Contains("tcp provider")
            || lower.Contains("sqlexception")
            || lower.Contains("microsoft.data.sqlclient")
            || lower.Contains("system.data.sqlclient")
            || lower.Contains("connection string")
            || lower.Contains("login failed")
            || (lower.Contains("timeout") && lower.Contains("sql")))
        {
            return "Błąd połączenia z bazą danych. Spróbuj ponownie później.";
        }

        return null;
    }

    private static string StatusFallback(int statusCode) =>
        statusCode switch
        {
            400 => "Nieprawidłowe dane (sprawdź parametry).",
            404 => "Brak danych dla podanych parametrów.",
            409 => "Konflikt danych.",
            401 => "Brak autoryzacji — zaloguj się ponownie.",
            403 => "Brak uprawnień do wykonania tej operacji.",
            _ => "Wystąpił błąd po stronie serwera. Spróbuj ponownie później."
        };
}