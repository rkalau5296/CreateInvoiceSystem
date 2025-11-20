namespace CreateInvoiceSystem.Nbp.Application.DTO;

public record CurrencyRate
{
    public string Currency { get; set; }
    public string Code { get; set; }
    public double Bid { get; set; }
    public double Ask { get; set; }
    public string EffectiveDate { get; set; }
    public double Mid { get; set; }
}