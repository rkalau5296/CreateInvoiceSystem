namespace CreateInvoiceSystem.Modules.Ksef.Mappers;

public static class KsefVatRateMapper
{
    public static string MapToKsefCode(string vatRate)
    {
        return vatRate switch
        {
            "23%" => "23",
            "8%" => "8",
            "5%" => "5",
            "0%" => "0 KR",
            "zw" => "zw",
            _ => throw new NotSupportedException(
                $"Nieobsługiwana stawka VAT dla KSeF: '{vatRate}'. " +
                "Dodaj mapowanie w KsefVatRateMapper, zanim wyślesz tę fakturę.")
        };
    }
}
