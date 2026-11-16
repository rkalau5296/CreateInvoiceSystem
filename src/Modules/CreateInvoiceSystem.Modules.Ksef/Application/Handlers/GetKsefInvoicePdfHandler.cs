using System.Globalization;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoicePdf;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using KSeF.Client.Core.Interfaces.Services;
using MediatR;

namespace CreateInvoiceSystem.Modules.Ksef.Application.Handlers;

public sealed class GetKsefInvoicePdfHandler(IKsefApiClient apiClient, IVerificationLinkService links, IKsefInvoicePdfGenerator generator)
    : IRequestHandler<GetKsefInvoicePdfRequest, byte[]>
{
    public async Task<byte[]> Handle(GetKsefInvoicePdfRequest request, CancellationToken cancellationToken)
    {
        var xml = await apiClient.DownloadInvoiceXmlBytesAsync(request.UserId, request.KsefNumber, cancellationToken);
        using var stream = new MemoryStream(xml, writable: false);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
        var document = XDocument.Load(reader);
        var seller = document.Root?.Elements().FirstOrDefault(element => element.Name.LocalName == "Podmiot1");
        var nip = seller?.Elements().FirstOrDefault(element => element.Name.LocalName == "DaneIdentyfikacyjne")
            ?.Elements().FirstOrDefault(element => element.Name.LocalName == "NIP")?.Value.Trim();
        var invoice = document.Root?.Elements().FirstOrDefault(element => element.Name.LocalName == "Fa");
        var issueDateText = invoice?.Elements().FirstOrDefault(element => element.Name.LocalName == "P_1")?.Value.Trim();

        if (nip is null || nip.Length != 10 || nip.Any(character => character is < '0' or > '9') ||
            !DateTime.TryParseExact(issueDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var issueDate))
            throw new InvalidOperationException("Nie można wygenerować kodu QR: faktura nie zawiera poprawnego NIP sprzedawcy lub daty wystawienia.");

        var hash = Convert.ToBase64String(SHA256.HashData(xml));
        var url = links.BuildInvoiceVerificationUrl(nip, issueDate, hash);
        return generator.Create(xml, request.KsefNumber, url);
    }
}
