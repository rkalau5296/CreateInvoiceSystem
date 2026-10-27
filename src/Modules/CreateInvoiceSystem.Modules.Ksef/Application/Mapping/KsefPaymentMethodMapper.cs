namespace CreateInvoiceSystem.Modules.Ksef.Application.Mapping;

public static class KsefPaymentMethodMapper
{
    public static int MapToKsefCode(string methodOfPayment)
    {
        return methodOfPayment switch
        {
            "Gotówka" => 1,
            "Karta" => 2,
            "Przelew" => 6,
            "BLIK" => 7,
            _ => throw new NotSupportedException(
                $"Nieobsługiwany sposób płatności dla KSeF: '{methodOfPayment}'. " +
                "Dodaj mapowanie w KsefPaymentMethodMapper, zanim wyślesz tę fakturę.")
        };
    }
}