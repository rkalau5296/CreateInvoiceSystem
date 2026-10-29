using System.Globalization;
using System.Xml.Linq;
using CreateInvoiceSystem.Modules.Ksef.Dto;

namespace CreateInvoiceSystem.Modules.Ksef.Mappers;

public static class InvoiceToFa3XmlMapper
{
    private static readonly XNamespace Fa3Ns = "http://crd.gov.pl/wzor/2025/06/25/13775/";
    private static readonly XNamespace EtdNs = "http://crd.gov.pl/xml/schematy/dziedzinowe/mf/2022/01/05/eD/DefinicjeTypy/";
    private static readonly XNamespace XsiNs = "http://www.w3.org/2001/XMLSchema-instance";

    public static XDocument MapToFa3Xml(KsefInvoiceXmlRequest request)
    {
        var sellerAddress = KsefAddressParser.Parse(request.SellerAddress);
        var clientAddress = KsefAddressParser.Parse(request.ClientAddress);

        var root = new XElement(Fa3Ns + "Faktura",
            new XAttribute(XNamespace.Xmlns + "etd", EtdNs),
            new XAttribute(XNamespace.Xmlns + "xsi", XsiNs),
            BuildNaglowek(),
            BuildPodmiot1(request, sellerAddress),
            BuildPodmiot2(request, clientAddress),
            BuildFa(request));

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), root);
    }

    private static XElement BuildNaglowek()
    {
        var kodFormularza = new XElement(Fa3Ns + "KodFormularza",
            new XAttribute("kodSystemowy", "FA (3)"),
            new XAttribute("wersjaSchemy", "1-0E"),
            "FA");

        return new XElement(Fa3Ns + "Naglowek",
            kodFormularza,
            new XElement(Fa3Ns + "WariantFormularza", 3),
            new XElement(Fa3Ns + "DataWytworzeniaFa", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")),
            new XElement(Fa3Ns + "SystemInfo", "CreateInvoiceSystem"));
    }

    private static XElement BuildPodmiot1(KsefInvoiceXmlRequest request, (string AdresL1, string AdresL2, string KodKraju) address)
    {
        return new XElement(Fa3Ns + "Podmiot1",
            new XElement(Fa3Ns + "DaneIdentyfikacyjne",
                new XElement(Fa3Ns + "NIP", request.SellerNip),
                new XElement(Fa3Ns + "Nazwa", request.SellerName)),
            new XElement(Fa3Ns + "Adres",
                new XElement(Fa3Ns + "KodKraju", address.KodKraju),
                new XElement(Fa3Ns + "AdresL1", address.AdresL1),
                new XElement(Fa3Ns + "AdresL2", address.AdresL2)));
    }

    private static XElement BuildPodmiot2(KsefInvoiceXmlRequest request, (string AdresL1, string AdresL2, string KodKraju) address)
    {
        var podmiot2 = new XElement(Fa3Ns + "Podmiot2",
            new XElement(Fa3Ns + "DaneIdentyfikacyjne",
                new XElement(Fa3Ns + "NIP", request.ClientNip),
                new XElement(Fa3Ns + "Nazwa", request.ClientName)),
            new XElement(Fa3Ns + "Adres",
                new XElement(Fa3Ns + "KodKraju", address.KodKraju),
                new XElement(Fa3Ns + "AdresL1", address.AdresL1),
                new XElement(Fa3Ns + "AdresL2", address.AdresL2)));

        if (!string.IsNullOrWhiteSpace(request.ClientEmail))
        {
            podmiot2.Add(new XElement(Fa3Ns + "DaneKontaktowe",
                new XElement(Fa3Ns + "Email", request.ClientEmail)));
        }

        return podmiot2;
    }

    private static XElement BuildFa(KsefInvoiceXmlRequest request)
    {
        var fa = new XElement(Fa3Ns + "Fa",
            new XElement(Fa3Ns + "KodWaluty", request.Currency),
            new XElement(Fa3Ns + "P_1", request.IssueDate.ToString("yyyy-MM-dd")),
            new XElement(Fa3Ns + "P_2", request.InvoiceNumber));

        AddVatRateTotals(fa, request);

        fa.Add(BuildAdnotacje());
        fa.Add(new XElement(Fa3Ns + "RodzajFaktury", "VAT"));

        var rowNumber = 1;
        foreach (var line in request.Lines)
        {
            fa.Add(BuildFaWiersz(line, rowNumber));
            rowNumber++;
        }

        fa.Add(BuildPlatnosc(request));

        return fa;
    }

    private static void AddVatRateTotals(XElement fa, KsefInvoiceXmlRequest request)
    {
        AddRateGroup(fa, request, "23%", Fa3Ns + "P_13_1", Fa3Ns + "P_14_1");
        AddRateGroup(fa, request, "8%", Fa3Ns + "P_13_2", Fa3Ns + "P_14_2");
        AddRateGroup(fa, request, "5%", Fa3Ns + "P_13_3", Fa3Ns + "P_14_3");
        AddNetOnlyGroup(fa, request, "0%", Fa3Ns + "P_13_6_1");
        AddNetOnlyGroup(fa, request, "zw", Fa3Ns + "P_13_7");

        fa.Add(new XElement(Fa3Ns + "P_15", FormatAmount(request.TotalGross)));
    }

    private static void AddRateGroup(XElement fa, KsefInvoiceXmlRequest request, string vatRate, XName netElementName, XName vatElementName)
    {
        var lines = request.Lines.Where(l => l.VatRate == vatRate).ToList();
        if (lines.Count == 0)
            return;

        fa.Add(new XElement(netElementName, FormatAmount(lines.Sum(l => l.NetAmount))));
        fa.Add(new XElement(vatElementName, FormatAmount(lines.Sum(l => l.VatAmount))));
    }

    private static void AddNetOnlyGroup(XElement fa, KsefInvoiceXmlRequest request, string vatRate, XName netElementName)
    {
        var lines = request.Lines.Where(l => l.VatRate == vatRate).ToList();
        if (lines.Count == 0)
            return;

        fa.Add(new XElement(netElementName, FormatAmount(lines.Sum(l => l.NetAmount))));
    }

    private static XElement BuildAdnotacje()
    {
        return new XElement(Fa3Ns + "Adnotacje",
            new XElement(Fa3Ns + "P_16", "2"),
            new XElement(Fa3Ns + "P_17", "2"),
            new XElement(Fa3Ns + "P_18", "2"),
            new XElement(Fa3Ns + "P_18A", "2"),
            new XElement(Fa3Ns + "Zwolnienie",
                new XElement(Fa3Ns + "P_19N", "1")),
            new XElement(Fa3Ns + "NoweSrodkiTransportu",
                new XElement(Fa3Ns + "P_22N", "1")),
            new XElement(Fa3Ns + "P_23", "2"),
            new XElement(Fa3Ns + "PMarzy",
                new XElement(Fa3Ns + "P_PMarzyN", "1")));
    }

    private static XElement BuildFaWiersz(KsefInvoiceLineXmlRequest line, int rowNumber)
    {
        return new XElement(Fa3Ns + "FaWiersz",
            new XElement(Fa3Ns + "NrWierszaFa", rowNumber),
            new XElement(Fa3Ns + "P_7", line.ProductName),
            new XElement(Fa3Ns + "P_8A", line.UnitOfMeasure),
            new XElement(Fa3Ns + "P_8B", FormatAmount(line.Quantity)),
            new XElement(Fa3Ns + "P_9A", FormatAmount(line.UnitPrice)),
            new XElement(Fa3Ns + "P_11", FormatAmount(line.NetAmount)),
            new XElement(Fa3Ns + "P_12", KsefVatRateMapper.MapToKsefCode(line.VatRate)));
    }

    private static XElement BuildPlatnosc(KsefInvoiceXmlRequest request)
    {
        var platnosc = new XElement(Fa3Ns + "Platnosc",
            new XElement(Fa3Ns + "TerminPlatnosci",
                new XElement(Fa3Ns + "Termin", request.PaymentDueDate.ToString("yyyy-MM-dd"))),
            new XElement(Fa3Ns + "FormaPlatnosci", KsefPaymentMethodMapper.MapToKsefCode(request.MethodOfPayment)));

        if (!string.IsNullOrWhiteSpace(request.BankAccountNumber))
        {
            platnosc.Add(new XElement(Fa3Ns + "RachunekBankowy",
                new XElement(Fa3Ns + "NrRB", request.BankAccountNumber)));
        }

        return platnosc;
    }

    private static string FormatAmount(decimal value) => value.ToString("F2", CultureInfo.InvariantCulture);
}