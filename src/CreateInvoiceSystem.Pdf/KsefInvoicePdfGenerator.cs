using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CreateInvoiceSystem.Pdf;

public sealed class KsefInvoicePdfGenerator
{
    private static readonly CultureInfo Polish = CultureInfo.GetCultureInfo("pl-PL");

    public byte[] Create(byte[] xml, string ksefNumber, string verificationUrl)
    {
        using var stream = new MemoryStream(xml, writable: false);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
        var root = XDocument.Load(reader).Root ?? throw new InvalidDataException("Dokument faktury KSeF jest pusty.");
        var invoice = Child(root, "Fa") ?? throw new InvalidDataException("Dokument nie zawiera sekcji faktury FA.");
        var seller = Child(root, "Podmiot1");
        var buyer = Child(root, "Podmiot2");
        var payment = Child(invoice, "Platnosc");
        var currency = Value(invoice, "KodWaluty") ?? "";
        var lines = Children(invoice, "FaWiersz").ToList();
        var amounts = TaxAmounts(invoice).ToList();
        var accounts = Children(payment, "RachunekBankowy").ToList();
        var qrEnvironment = VerificationEnvironment(verificationUrl);
        using var qrData = QRCodeGenerator.GenerateQrCode(verificationUrl, QRCodeGenerator.ECCLevel.M);
        using var qr = new PngByteQRCode(qrData);
        var qrImage = qr.GetGraphic(6);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(style => style.FontFamily("Lato").FontSize(8));
                if (qrEnvironment is not null)
                    page.Background().AlignCenter().AlignMiddle().Rotate(-45).Text(qrEnvironment)
                        .FontSize(48).Bold().FontColor("#f0f0f0");

                page.Content().Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text(text =>
                    {
                        text.Span("Krajowy System ").FontSize(17);
                        text.Span("e-").FontSize(17).Bold().FontColor("#d50000");
                        text.Span("Faktur").FontSize(17).Bold();
                    });
                    column.Item().AlignRight().Column(header =>
                    {
                        header.Item().AlignRight().Text("Numer Faktury:").FontSize(7).FontColor(Colors.Grey.Darken1);
                        header.Item().AlignRight().Text(Value(invoice, "P_2") ?? "—").FontSize(17).Bold();
                        header.Item().AlignRight().Text(InvoiceType(Value(invoice, "RodzajFaktury"))).FontSize(9);
                        header.Item().AlignRight().Text($"Numer KSeF: {ksefNumber}").SemiBold();
                        if (KsefDate(ksefNumber) is { } registered)
                            header.Item().AlignRight().Text($"Data nadania numeru KSeF: {registered}").SemiBold();
                    });

                    column.Item().Element(Separator);
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Element(container => Party(container, "Sprzedawca", seller, false));
                        row.ConstantItem(15);
                        row.RelativeItem().Element(container => Party(container, "Nabywca", buyer, true));
                    });
                    column.Item().Element(Separator);
                    column.Item().Text("Szczegóły").FontSize(10).SemiBold();
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(details =>
                        {
                            details.Item().Text($"Data wystawienia, z zastrzeżeniem art. 106na ust. 1 ustawy: {FormatDate(Value(invoice, "P_1"))}");
                            if (!string.IsNullOrEmpty(currency))
                                details.Item().Text($"Kod waluty: {currency}");
                            if (Value(invoice, "P_1M") is { } place)
                                details.Item().Text($"Miejsce wystawienia: {place}");
                        });
                        row.ConstantItem(15);
                        row.RelativeItem().Column(details =>
                        {
                            if (Value(invoice, "P_6") is { } saleDate)
                                details.Item().Text($"Data dokonania lub zakończenia dostawy towarów lub wykonania usługi: {FormatDate(saleDate)}");
                        });
                    });
                    column.Item().Element(Separator);
                    column.Item().Text("Pozycje").FontSize(10).SemiBold();
                    if (!string.IsNullOrEmpty(currency))
                        column.Item().Text($"Faktura wystawiona w walucie {currency}").FontSize(7);
                    if (lines.Count > 0)
                        column.Item().Element(container => InvoiceLines(container, lines));
                    if (Value(invoice, "P_15") is { } total)
                        column.Item().AlignRight().Text($"Kwota należności ogółem: {FormatAmount(total)} {currency}").FontSize(10).SemiBold();

                    if (amounts.Count > 0)
                    {
                        column.Item().Text("Podsumowanie stawek podatku").FontSize(10).SemiBold();
                        column.Item().Element(container => TaxTable(container, amounts));
                        foreach (var amount in amounts.Where(x => x.TaxPln is not null))
                            column.Item().AlignRight().Text($"Kwota podatku ({amount.Label}): {FormatAmount(amount.TaxPln)} PLN");
                    }
                    if (Value(invoice, "P_13_11") is { } margin)
                        column.Item().Text($"Wartość sprzedaży w procedurze marży: {FormatAmount(margin)} {currency}");

                    if (payment is not null)
                    {
                        column.Item().Text("Płatność").FontSize(10).SemiBold();
                        if (Value(payment, "FormaPlatnosci") is { } method)
                            column.Item().Text($"Forma płatności: {PaymentMethod(method)}");
                        if (Value(payment, "PlatnoscInna") == "1" && Value(payment, "OpisPlatnosci") is { } description)
                            column.Item().Text($"Forma płatności: {description}");
                        foreach (var term in Children(payment, "TerminPlatnosci"))
                        {
                            if (Value(term, "Termin") is { } date)
                                column.Item().Width(230).Column(termColumn =>
                                {
                                    termColumn.Item().Element(TableHeader).Text("Termin płatności").FontSize(7);
                                    termColumn.Item().Element(TableCell).Text(FormatDate(date)).FontSize(7);
                                });
                        }
                        foreach (var account in accounts)
                        {
                            column.Item().Text("Numer rachunku bankowego").FontSize(10).SemiBold();
                            column.Item().Width(230).Element(container => KeyValueTable(container,
                            [
                                ("Pełny numer rachunku", FormatAccount(Value(account, "NrRB"))),
                                ("Kod SWIFT", Value(account, "SWIFT") ?? ""),
                                ("Rachunek własny banku", Value(account, "RachunekWlasnyBanku") == "1" ? "TAK" : ""),
                                ("Nazwa banku", Value(account, "NazwaBanku") ?? ""),
                                ("Opis rachunku", Value(account, "OpisRachunku") ?? "")
                            ]));
                        }
                    }

                    foreach (var description in Children(invoice, "DodatkowyOpis"))
                    {
                        var key = Value(description, "Klucz");
                        var value = Value(description, "Wartosc");
                        if (value is not null)
                            column.Item().Text(string.IsNullOrEmpty(key) ? value : $"{key}: {value}");
                    }

                    column.Item().PageBreak();
                    column.Item().Element(Separator);
                    column.Item().Text("Sprawdź, czy Twoja faktura znajduje się w KSeF!").SemiBold();
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(160).Column(code =>
                        {
                            code.Item().Width(150).Height(150).Image(qrImage);
                            code.Item().Text(ksefNumber).FontSize(6);
                        });
                        row.RelativeItem().PaddingLeft(15).Column(info =>
                        {
                            info.Item().Text("Nie możesz zeskanować kodu z obrazka? Kliknij w link weryfikacyjny i przejdź do weryfikacji faktury!").FontSize(7);
                            info.Item().PaddingTop(5).Hyperlink(verificationUrl).Text(verificationUrl).FontSize(7).FontColor(Colors.Blue.Medium);
                        });
                    });
                    if (Value(Child(root, "Naglowek"), "SystemInfo") is { } systemInfo)
                        column.Item().PaddingTop(10).Text($"Wytworzona w: {systemInfo}").FontSize(7);
                });
                page.Footer().AlignRight().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" z ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static void Party(IContainer container, string label, XElement? party, bool isBuyer)
    {
        var identification = Child(party, "DaneIdentyfikacyjne");
        var address = Child(party, "Adres");
        container.Column(column =>
        {
            column.Spacing(2);
            column.Item().PaddingBottom(4).Text(label).FontSize(10).SemiBold();
            if (Value(identification, "NIP") is { } nip) column.Item().Text($"NIP: {nip}");
            if (Value(identification, "NrVatUE") is { } vatId) column.Item().Text($"Nr VAT UE: {vatId}");
            if (Value(identification, "NrID") is { } id) column.Item().Text($"Numer identyfikacyjny: {id}");
            if (Value(identification, "Nazwa") is { } name) column.Item().Text($"Nazwa: {name}");
            if (address is not null)
            {
                column.Item().PaddingTop(5).Text("Adres").SemiBold();
                foreach (var line in address.Elements().Where(x => x.Name.LocalName.StartsWith("AdresL", StringComparison.Ordinal)))
                    column.Item().Text(line.Value.Trim());
                if (Value(address, "KodKraju") is { } country)
                    column.Item().Text(country == "PL" ? "Polska" : country);
            }
            if (Value(Child(party, "DaneKontaktowe"), "Email") is { } email)
                column.Item().Text($"E-mail: {email}");
            if (isBuyer)
            {
                if (Value(party, "JST") is { } jst) column.Item().PaddingTop(5).Text($"Faktura dotyczy jednostki podrzędnej JST: {YesNo(jst)}").SemiBold();
                if (Value(party, "GV") is { } gv) column.Item().Text($"Faktura dotyczy członka grupy GV: {YesNo(gv)}").SemiBold();
            }
        });
    }

    private static void InvoiceLines(IContainer container, List<XElement> lines)
    {
        var hasNet = lines.Any(line => Value(line, "P_11") is not null);
        var hasGross = lines.Any(line => Value(line, "P_11A") is not null);
        var hasNetPrice = lines.Any(line => Value(line, "P_9A") is not null);
        var hasGrossPrice = lines.Any(line => Value(line, "P_9B") is not null);
        var priceHeader = hasNetPrice && hasGrossPrice ? "Cena jedn." : hasGrossPrice ? "Cena jedn. brutto" : "Cena jedn. netto";
        var amountHeader = hasNet && hasGross ? "Wartość sprzedaży" : hasGross ? "Wartość sprzedaży brutto" : "Wartość sprzedaży netto";
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(24);
                columns.RelativeColumn(3);
                columns.RelativeColumn(1.35f);
                columns.RelativeColumn(0.7f);
                columns.RelativeColumn(0.7f);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(1.6f);
            });
            table.Header(header =>
            {
                foreach (var label in new[] { "Lp.", "Nazwa towaru lub usługi", priceHeader, "Ilość", "Miara", "Stawka podatku", amountHeader })
                    header.Cell().Element(TableHeader).Text(label).FontSize(7).SemiBold();
            });
            foreach (var line in lines)
            {
                var price = Value(line, "P_9A") ?? Value(line, "P_9B");
                var amount = Value(line, "P_11") ?? Value(line, "P_11A");
                var values = new[]
                {
                    Value(line, "NrWierszaFa") ?? "", Value(line, "P_7") ?? "", FormatPrice(price),
                    Value(line, "P_8B") ?? "", Value(line, "P_8A") ?? "", FormatRate(Value(line, "P_12")), FormatAmount(amount)
                };
                for (var index = 0; index < values.Length; index++)
                {
                    var cell = table.Cell().Element(TableCell);
                    if (index is 2 or 3 or 6) cell = cell.AlignRight();
                    cell.Text(values[index]).FontSize(7);
                }
            }
        });
    }

    private static void TaxTable(IContainer container, List<TaxAmount> amounts)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(24);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.3f);
                columns.RelativeColumn(1.3f);
                columns.RelativeColumn(1.3f);
            });
            table.Header(header =>
            {
                foreach (var label in new[] { "Lp.", "Stawka podatku", "Kwota netto", "Kwota podatku", "Kwota brutto" })
                    header.Cell().Element(TableHeader).Text(label).FontSize(7).SemiBold();
            });
            for (var index = 0; index < amounts.Count; index++)
            {
                var amount = amounts[index];
                table.Cell().Element(TableCell).Text((index + 1).ToString(Polish));
                table.Cell().Element(TableCell).Text(amount.Label);
                table.Cell().Element(TableCell).AlignRight().Text(FormatAmount(amount.Net));
                table.Cell().Element(TableCell).AlignRight().Text(FormatAmount(amount.Tax));
                table.Cell().Element(TableCell).AlignRight().Text(SumAmounts(amount.Net, amount.Tax));
            }
        });
    }

    private static void KeyValueTable(IContainer container, IEnumerable<(string Label, string Value)> values)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.ConstantColumn(90); columns.RelativeColumn(); });
            foreach (var (label, value) in values)
            {
                table.Cell().Element(TableHeader).Text(label).FontSize(7);
                table.Cell().Element(TableCell).Text(value).FontSize(7);
            }
        });
    }

    private static IEnumerable<TaxAmount> TaxAmounts(XElement invoice)
    {
        foreach (var net in invoice.Elements().Where(x =>
            x.Name.LocalName.StartsWith("P_13_", StringComparison.Ordinal) && x.Name.LocalName != "P_13_11"))
        {
            var suffix = net.Name.LocalName["P_13_".Length..];
            var tax = Value(invoice, "P_14_" + suffix);
            yield return new TaxAmount(TaxLabel(suffix), net.Value.Trim(), tax, Value(invoice, "P_14_" + suffix + "W"));
        }
    }

    private static string TaxLabel(string suffix) => suffix switch
    {
        "1" => "23% lub 22%",
        "2" => "8% lub 7%",
        "3" => "5%",
        "4" => "Ryczałt (taksówki)",
        "5" => "Procedura szczególna",
        "6_1" => "0% (kraj)",
        "6_2" => "0% (WDT)",
        "6_3" => "0% (eksport)",
        "7" => "zwolniona",
        "8" => "poza terytorium kraju",
        "9" => "usługi art. 100 ust. 1 pkt 4",
        "10" => "odwrotne obciążenie",
        _ => $"P_13_{suffix}"
    };

    private static string FormatRate(string? value) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _) ? value + "%" : value ?? "";

    private static string FormatAmount(string? value) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
            ? number.ToString("N2", Polish) : value ?? "";

    private static string FormatPrice(string? value)
    {
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
            return value ?? "";
        var scale = (decimal.GetBits(number)[3] >> 16) & 0x7f;
        return number.ToString($"N{Math.Max(2, scale)}", Polish);
    }

    private static string SumAmounts(string net, string? tax) =>
        decimal.TryParse(net, NumberStyles.Number, CultureInfo.InvariantCulture, out var netValue) &&
        decimal.TryParse(tax, NumberStyles.Number, CultureInfo.InvariantCulture, out var taxValue)
            ? (netValue + taxValue).ToString("N2", Polish) : "";

    private static string FormatDate(string? value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date.ToString("dd.MM.yyyy", Polish) : value ?? "";

    private static string? KsefDate(string number)
    {
        var parts = number.Split('-');
        return parts.Length >= 3 && DateOnly.TryParseExact(parts[1], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date.ToString("dd.MM.yyyy", Polish) : null;
    }

    private static string FormatAccount(string? value)
    {
        if (value is null) return "";
        if (value.Length != 26 || value.Any(c => c is < '0' or > '9')) return value;
        return value[..2] + " " + string.Join(" ", Enumerable.Range(0, 6).Select(i => value.Substring(2 + i * 4, 4)));
    }

    private static string? VerificationEnvironment(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Host.ToLowerInvariant() switch
        {
            "qr-test.ksef.mf.gov.pl" => "Środowisko Testowe",
            "qr-demo.ksef.mf.gov.pl" => "Środowisko Demo",
            _ => null
        } : null;

    private static string InvoiceType(string? type) => type switch
    {
        "VAT" or null => "Faktura podstawowa",
        "KOR" => "Faktura korygująca",
        "ZAL" => "Faktura zaliczkowa",
        "ROZ" => "Faktura rozliczeniowa",
        "UPR" => "Faktura uproszczona",
        "KOR_ZAL" => "Faktura korygująca zaliczkowa",
        "KOR_ROZ" => "Faktura korygująca rozliczeniowa",
        _ => type
    };

    private static string PaymentMethod(string code) => code switch
    {
        "1" => "Gotówka",
        "2" => "Karta",
        "3" => "Bon",
        "4" => "Czek",
        "5" => "Kredyt",
        "6" => "Przelew",
        "7" => "Płatność mobilna",
        _ => code
    };

    private static string YesNo(string value) => value switch { "1" => "TAK", "2" => "NIE", _ => value };
    private static IContainer Separator(IContainer container) => container.Height(0.5f).Background(Colors.Grey.Lighten1);
    private static IContainer TableHeader(IContainer container) => container.Background("#f4f5f6").Border(0.5f).BorderColor("#bcbcbc").Padding(3);
    private static IContainer TableCell(IContainer container) => container.Border(0.5f).BorderColor("#bcbcbc").Padding(3);
    private static XElement? Child(XElement? element, string name) => element?.Elements().FirstOrDefault(x => x.Name.LocalName == name);
    private static IEnumerable<XElement> Children(XElement? element, string name) => element?.Elements().Where(x => x.Name.LocalName == name) ?? [];
    private static string? Value(XElement? element, string name) => Child(element, name)?.Value.Trim() is { Length: > 0 } value ? value : null;
    private sealed record TaxAmount(string Label, string Net, string? Tax, string? TaxPln);
}
