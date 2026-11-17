using System.Globalization;
using System.Xml.Linq;
using CreateInvoiceSystem.Modules.Ksef.Dto;

namespace CreateInvoiceSystem.Modules.Ksef.Mappers;

public static class KsefInvoiceXmlMapper
{
    public static KsefInvoiceDetailsDto ToDetailsDto(string ksefNumber, string xml)
    {
        var document = XDocument.Parse(xml);
        var invoice = document.Descendants().FirstOrDefault(element => element.Name.LocalName == "Fa")
            ?? throw new InvalidDataException("Dokument XML z KSeF nie zawiera sekcji faktury FA.");

        var seller = MapParty(document, "Podmiot1");
        var buyer = MapParty(document, "Podmiot2");
        var lines = invoice.Descendants()
            .Where(element => element.Name.LocalName == "FaWiersz")
            .Select((element, index) => MapLine(element, index + 1))
            .ToList();

        var netFromTotals = SumAmountFields(invoice, "P_13_");
        var vatFromTotals = SumAmountFields(invoice, "P_14_");
        var grossFromTotals = ParseAmount(GetValue(invoice, "P_15"));
        var netAmount = netFromTotals ?? lines.Sum(line => line.NetAmount);
        var vatAmount = vatFromTotals ?? (grossFromTotals - netAmount) ?? 0;
        var grossAmount = grossFromTotals ?? netAmount + vatAmount;
        var payment = document.Descendants().FirstOrDefault(element => element.Name.LocalName == "Platnosc");
        var paymentDueDate = ParseDate(GetDescendantValue(payment, "Termin"))
            ?? ParseDate(GetValue(payment, "TerminPlatnosci"))
            ?? ParseDate(GetDescendantValue(document.Root, "Termin"))
            ?? ParseDate(GetDescendantValue(document.Root, "DataPlatnosci"));
        var bankAccount = GetDescendantValue(payment, "NrRB")
            ?? GetDescendantValue(document.Root, "NrRB")
            ?? GetDescendantValue(document.Root, "NrRachunku");

        return new KsefInvoiceDetailsDto(
            ksefNumber,
            GetValue(invoice, "P_2") ?? string.Empty,
            ParseDate(GetValue(invoice, "P_1")),
            ParseDate(GetValue(invoice, "P_6")),
            GetValue(invoice, "KodWaluty") ?? "PLN",
            seller,
            buyer,
            lines,
            netAmount,
            vatAmount,
            grossAmount,
            paymentDueDate,
            GetValue(payment, "FormaPlatnosci"),
            bankAccount);
    }

    private static KsefInvoicePartyDto MapParty(XDocument document, string elementName)
    {
        var party = document.Descendants().FirstOrDefault(element => element.Name.LocalName == elementName);
        var identification = FindChild(party, "DaneIdentyfikacyjne");
        var address = FindChild(party, "Adres");
        var addressLines = address?.Elements()
            .Where(element => element.Name.LocalName.StartsWith("AdresL", StringComparison.Ordinal))
            .Select(element => element.Value.Trim())
            .Where(value => value.Length > 0);

        return new KsefInvoicePartyDto(
            GetValue(identification, "Nazwa"),
            GetValue(identification, "NIP") ?? GetValue(identification, "NrVatUE"),
            addressLines is null ? null : string.Join(", ", addressLines),
            GetValue(FindChild(party, "DaneKontaktowe"), "Email"));
    }

    private static KsefInvoiceLineDto MapLine(XElement element, int index)
    {
        return new KsefInvoiceLineDto(
            ParseInteger(GetValue(element, "NrWierszaFa")) ?? index,
            GetValue(element, "P_7"),
            GetValue(element, "P_8A"),
            ParseAmount(GetValue(element, "P_8B")) ?? 0,
            ParseAmount(GetValue(element, "P_9A") ?? GetValue(element, "P_9B")) ?? 0,
            ParseAmount(GetValue(element, "P_11")) ?? 0,
            FormatVatRate(GetValue(element, "P_12")));
    }

    private static XElement? FindChild(XElement? element, string name) =>
        element?.Elements().FirstOrDefault(child => child.Name.LocalName == name);

    private static string? GetValue(XElement? element, string name) =>
        FindChild(element, name)?.Value.Trim() is { Length: > 0 } value ? value : null;

    private static string? GetDescendantValue(XElement? element, string name) =>
        element?.Descendants().FirstOrDefault(child => child.Name.LocalName == name)?.Value.Trim() is { Length: > 0 } value
            ? value
            : null;

    private static decimal? SumAmountFields(XElement element, string prefix)
    {
        var values = element.Elements()
            .Where(child =>
            {
                var name = child.Name.LocalName;
                var suffix = name.StartsWith(prefix, StringComparison.Ordinal) ? name[prefix.Length..] : string.Empty;
                return suffix.Length > 0 && suffix.All(character => char.IsDigit(character) || character == '_');
            })
            .Select(child => ParseAmount(child.Value))
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        return values.Count == 0 ? null : values.Sum();
    }

    private static decimal? ParseAmount(string? value) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
            ? amount
            : null;

    private static int? ParseInteger(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            ? number
            : null;

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;

    private static string? FormatVatRate(string? value) => value switch
    {
        "23" => "23%",
        "8" => "8%",
        "5" => "5%",
        "0" => "0%",
        _ => value
    };
}
