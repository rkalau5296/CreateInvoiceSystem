namespace CreateInvoiceSystem.Modules.Ksef.Mappers;

public static class KsefCountryMapper
{
    public static string MapToIsoCode(string country)
    {
        if (string.IsNullOrWhiteSpace(country))
            return "PL";

        return country.Trim().ToLowerInvariant() switch
        {
            "polska" or "poland" or "pl" => "PL",
            "niemcy" or "germany" or "de" => "DE",
            "czechy" or "czech republic" or "cz" => "CZ",
            "słowacja" or "slovakia" or "sk" => "SK",
            "ukraina" or "ukraine" or "ua" => "UA",
            _ => throw new NotSupportedException(
                $"Nieobsługiwany kraj dla KSeF: '{country}'. " +
                "Dodaj mapowanie w KsefCountryMapper, zanim wyślesz tę fakturę.")
        };
    }
}