using System.Security.Cryptography;
using System.Text;
using CreateInvoiceSystem.API.RestServices;
using CreateInvoiceSystem.Modules.Ksef.Application.Handlers;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoicePdf;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using CreateInvoiceSystem.Pdf;
using KSeF.Client.Api.Services;
using KSeF.Client.Core.Infrastructure.Rest;
using KSeF.Client.Core.Interfaces.Services;
using KSeF.Client.DI;
using Moq;
using UglyToad.PdfPig;
using SkiaSharp;
using ZXing;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class KsefInvoicePdfTests
{
    private const string InvoiceXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <Faktura xmlns="urn:test">
          <Naglowek><KodFormularza kodSystemowy="FA (3)">FA</KodFormularza><SystemInfo>Testowy system</SystemInfo></Naglowek>
          <Podmiot1><DaneIdentyfikacyjne><NIP>1234567890</NIP><Nazwa>Sprzedawca testowy</Nazwa></DaneIdentyfikacyjne><Adres><AdresL1>Ulica Testowa 1</AdresL1></Adres></Podmiot1>
          <Podmiot2><DaneIdentyfikacyjne><NIP>9876543210</NIP><Nazwa>Nabywca testowy</Nazwa></DaneIdentyfikacyjne><JST>2</JST><GV>2</GV></Podmiot2>
          <Fa>
            <KodWaluty>EUR</KodWaluty><P_1>2026-01-02</P_1><P_2>FV/TEST/1</P_2><P_6>2026-01-01</P_6>
            <P_13_1>100.00</P_13_1><P_14_1>23.00</P_14_1><P_14_1W>99.75</P_14_1W><P_15>123.00</P_15>
            <FaWiersz><NrWierszaFa>1</NrWierszaFa><P_7>Przykładowa usługa</P_7><P_8A>szt.</P_8A><P_8B>1</P_8B><P_9A>100.1234</P_9A><P_11>100.00</P_11><P_12>23</P_12></FaWiersz>
            <Platnosc><TerminPlatnosci><Termin>2026-02-01</Termin></TerminPlatnosci><FormaPlatnosci>6</FormaPlatnosci><RachunekBankowy><NrRB>12345678901234567890123456</NrRB></RachunekBankowy></Platnosc>
            <DodatkowyOpis><Klucz>Własne pole</Klucz><Wartosc>Ważna dodatkowa treść</Wartosc></DodatkowyOpis>
          </Fa>
        </Faktura>
        """;

    [Fact]
    public async Task Pdf_handler_hashes_original_bytes_and_forwards_user_and_context()
    {
        var bytes = Encoding.UTF8.Preamble.ToArray().Concat(Encoding.UTF8.GetBytes(InvoiceXml)).ToArray();
        var api = new Mock<IKsefApiClient>(MockBehavior.Strict);
        var generator = new Mock<IKsefInvoicePdfGenerator>(MockBehavior.Strict);
        var links = new VerificationLinkService(new KSeFClientOptions { BaseUrl = "https://api-test.ksef.mf.gov.pl" });
        var hash = Convert.ToBase64String(SHA256.HashData(bytes)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var expectedUrl = $"https://qr-test.ksef.mf.gov.pl/invoice/1234567890/02-01-2026/{hash}";
        api.Setup(x => x.DownloadInvoiceXmlBytesAsync(12, "ksef-number", It.IsAny<CancellationToken>())).ReturnsAsync(bytes);
        generator.Setup(x => x.Create(bytes, "ksef-number", expectedUrl)).Returns([1, 2, 3]);
        var handler = new GetKsefInvoicePdfHandler(api.Object, links, generator.Object);

        var result = await handler.Handle(new GetKsefInvoicePdfRequest(12, "ksef-number"), TestContext.Current.CancellationToken);

        Assert.Equal(new byte[] { 1, 2, 3 }, result);
        api.VerifyAll();
        generator.VerifyAll();
    }

    [Fact]
    public async Task Missing_seller_nip_does_not_create_misleading_qr()
    {
        var api = new Mock<IKsefApiClient>();
        api.Setup(x => x.DownloadInvoiceXmlBytesAsync(12, "number", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(InvoiceXml.Replace("<NIP>1234567890</NIP>", "")));
        var generator = new Mock<IKsefInvoicePdfGenerator>(MockBehavior.Strict);
        var handler = new GetKsefInvoicePdfHandler(api.Object, Mock.Of<IVerificationLinkService>(), generator.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new GetKsefInvoicePdfRequest(12, "number"), TestContext.Current.CancellationToken));
        generator.VerifyNoOtherCalls();
    }

    [Fact]
    public void Pdf_follows_reference_layout_and_contains_verifiable_qr()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        const string verificationUrl = "https://qr-test.ksef.mf.gov.pl/invoice/test";
        const string ksefNumber = "1234567890-20260103-test-number";
        var pdf = new KsefInvoicePdfGenerator().Create(Encoding.UTF8.GetBytes(InvoiceXml), ksefNumber, verificationUrl);
        using var document = PdfDocument.Open(pdf);
        var text = string.Join(" ", document.GetPages().Select(page => page.Text));

        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(pdf, 0, 5));
        Assert.Equal(2, document.NumberOfPages);
        foreach (var value in new[] { "Krajowy System", "FV/TEST/1", ksefNumber, "Sprzedawca testowy", "Nabywca testowy",
                     "Przykładowa usługa", "01.02.2026", "03.01.2026", "12 3456 7890 1234 5678 9012 3456", "EUR",
                     "Ważna dodatkowa treść", "Podsumowanie stawek podatku", "99,75 PLN", "100,1234", "23%" })
            Assert.Contains(value, text);
        Assert.DoesNotContain("Pełne dane dokumentu XML", text);
        Assert.DoesNotContain("Faktura/Fa/", text);
        Assert.Empty(document.GetPage(1).GetImages());
        var image = Assert.Single(document.GetPage(2).GetImages());
        Assert.True(image.TryGetPng(out var png));
        using var bitmap = SKBitmap.Decode(png);
        var pixels = new byte[bitmap.Width * bitmap.Height * 3];
        for (var y = 0; y < bitmap.Height; y++)
        for (var x = 0; x < bitmap.Width; x++)
        {
            var pixel = bitmap.GetPixel(x, y);
            var offset = (y * bitmap.Width + x) * 3;
            pixels[offset] = pixel.Red;
            pixels[offset + 1] = pixel.Green;
            pixels[offset + 2] = pixel.Blue;
        }
        var source = new RGBLuminanceSource(pixels, bitmap.Width, bitmap.Height, RGBLuminanceSource.BitmapFormat.RGB24);
        Assert.Equal(verificationUrl, new BarcodeReaderGeneric().Decode(source)?.Text);
    }

    [Fact]
    public async Task Raw_invoice_client_preserves_bytes_and_uses_authenticated_route()
    {
        var expected = new byte[] { 0xEF, 0xBB, 0xBF, 0x3C, 0x61, 0x2F, 0x3E };
        var transport = new RecordingHandler(expected);
        using var http = new HttpClient(transport) { BaseAddress = new Uri("https://example.test") };
        var client = new KsefInvoiceBytesClient(http, new RouteBuilder("v2"));

        var bytes = await client.DownloadAsync("invoice-123", "test-access-token", TestContext.Current.CancellationToken);

        Assert.Equal(expected, bytes);
        Assert.Equal("/v2/invoices/ksef/invoice-123", transport.Path);
        Assert.Equal("Bearer", transport.Scheme);
        Assert.Equal("test-access-token", transport.Token);
        Assert.Equal("application/xml", transport.Accept);
    }

    [Fact]
    public void Long_invoice_preserves_late_lines_across_pages()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var rows = string.Concat(Enumerable.Range(1, 150).Select(i =>
            $"<FaWiersz><NrWierszaFa>{i}</NrWierszaFa><P_7>Pozycja testowa {i}</P_7><P_11>1.00</P_11></FaWiersz>"));
        var xml = InvoiceXml.Replace("<DodatkowyOpis>", rows + "<DodatkowyOpis>");
        var pdf = new KsefInvoicePdfGenerator().Create(Encoding.UTF8.GetBytes(xml), "test-number", "https://qr-test.ksef.mf.gov.pl/invoice/test");
        using var document = PdfDocument.Open(pdf);
        var text = string.Join(" ", document.GetPages().Select(page => page.Text));

        Assert.True(document.NumberOfPages > 2);
        Assert.Contains("Pozycja testowa 150", text);
        Assert.Contains("Ważna dodatkowa treść", text);
    }

    private sealed class RecordingHandler(byte[] responseBytes) : HttpMessageHandler
    {
        public string? Path { get; private set; }
        public string? Scheme { get; private set; }
        public string? Token { get; private set; }
        public string? Accept { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Path = request.RequestUri?.AbsolutePath;
            Scheme = request.Headers.Authorization?.Scheme;
            Token = request.Headers.Authorization?.Parameter;
            Accept = request.Headers.Accept.Single().MediaType;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new ByteArrayContent(responseBytes) });
        }
    }
}
