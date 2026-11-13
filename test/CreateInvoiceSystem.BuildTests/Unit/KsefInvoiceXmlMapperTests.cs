using CreateInvoiceSystem.Modules.Ksef.Mappers;
using FluentAssertions;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class KsefInvoiceXmlMapperTests
{
    [Fact]
    public void ToDetailsDto_ShouldMapFa3InvoiceDetails()
    {
        const string xml = """
            <Faktura xmlns="http://crd.gov.pl/wzor/2025/06/25/13775/">
              <Podmiot1>
                <DaneIdentyfikacyjne><NIP>1112223344</NIP><Nazwa>Sprzedawca Sp. z o.o.</Nazwa></DaneIdentyfikacyjne>
                <Adres><AdresL1>ul. Testowa 1</AdresL1><AdresL2>00-001 Warszawa</AdresL2></Adres>
              </Podmiot1>
              <Podmiot2>
                <DaneIdentyfikacyjne><NIP>5556667788</NIP><Nazwa>Nabywca S.A.</Nazwa></DaneIdentyfikacyjne>
                <Adres><AdresL1>ul. Przykładowa 2</AdresL1></Adres>
              </Podmiot2>
              <Fa>
                <KodWaluty>PLN</KodWaluty>
                <P_1>2026-10-01</P_1>
                <P_2>FV/10/2026</P_2>
                <P_6>2026-09-30</P_6>
                <P_13_1>100.00</P_13_1>
                <P_14_1>23.00</P_14_1>
                <P_14_1W>999.00</P_14_1W>
                <P_15>123.00</P_15>
                <FaWiersz>
                  <NrWierszaFa>1</NrWierszaFa><P_7>Usługa</P_7><P_8A>szt.</P_8A>
                  <P_8B>2</P_8B><P_9A>50.00</P_9A><P_11>100.00</P_11><P_12>23</P_12>
                </FaWiersz>
              </Fa>
              <Platnosc>
                <TerminPlatnosci><Termin>2026-10-31</Termin></TerminPlatnosci>
                <FormaPlatnosci>6</FormaPlatnosci>
                <RachunekBankowy><NrRB>1234567890</NrRB></RachunekBankowy>
              </Platnosc>
            </Faktura>
            """;

        var result = KsefInvoiceXmlMapper.ToDetailsDto("KSEF-123", xml);

        result.KsefNumber.Should().Be("KSEF-123");
        result.InvoiceNumber.Should().Be("FV/10/2026");
        result.IssueDate.Should().Be(new DateOnly(2026, 10, 1));
        result.SaleDate.Should().Be(new DateOnly(2026, 9, 30));
        result.Seller.Name.Should().Be("Sprzedawca Sp. z o.o.");
        result.Seller.Nip.Should().Be("1112223344");
        result.Seller.Address.Should().Be("ul. Testowa 1, 00-001 Warszawa");
        result.Buyer.Name.Should().Be("Nabywca S.A.");
        result.Lines.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                Number = 1,
                Description = "Usługa",
                UnitOfMeasure = "szt.",
                Quantity = 2m,
                UnitPrice = 50m,
                NetAmount = 100m,
                VatRate = "23%"
            });
        result.NetAmount.Should().Be(100m);
        result.VatAmount.Should().Be(23m);
        result.GrossAmount.Should().Be(123m);
        result.PaymentDueDate.Should().Be(new DateOnly(2026, 10, 31));
        result.PaymentMethod.Should().Be("6");
        result.BankAccount.Should().Be("1234567890");
    }
}
