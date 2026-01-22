namespace CreateInvoiceSystem.Frontend.Models;

public class GetActualCurrencyRatesResponse
{
    public List<CurrencyRatesTable> Data { get; set; } = new();
}
