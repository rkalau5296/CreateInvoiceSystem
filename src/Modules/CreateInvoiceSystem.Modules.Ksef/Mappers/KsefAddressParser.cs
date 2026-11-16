namespace CreateInvoiceSystem.Modules.Ksef.Mappers;

public static class KsefAddressParser
{
    public static (string AdresL1, string AdresL2, string KodKraju) Parse(string formattedAddress)
    {
        if (string.IsNullOrWhiteSpace(formattedAddress))
            throw new ArgumentException("Adres nie może być pusty przy wysyłce do KSeF.", nameof(formattedAddress));

        var parts = formattedAddress.Split(',');

        if (parts.Length < 4)
            throw new NotSupportedException(
                $"Nieoczekiwany format adresu dla KSeF: '{formattedAddress}'. " +
                "Oczekiwano formatu 'Ulica Numer, Miasto, KodPocztowy, Kraj'.");

        var country = parts[^1].Trim();
        var postalCode = parts[^2].Trim();
        var city = parts[^3].Trim();
        var streetAndNumber = string.Join(',', parts[..^3]).Trim();

        return (
            AdresL1: streetAndNumber,
            AdresL2: $"{postalCode} {city}",
            KodKraju: KsefCountryMapper.MapToIsoCode(country)
        );
    }
}