namespace CreateInvoiceSystem.Nbp.Application.DTO;

public record CurrencyRatesTable
{
    public string Table { get; set; }
    public string TradingDate { get; set; }
    public string EffectiveDate { get; set; }
    public string CurrencyCode { get; set; } 
    public string CurrencyName { get; set; }
    public List<CurrencyRate> Rates { get; set; }
}