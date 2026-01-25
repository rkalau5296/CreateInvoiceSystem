namespace CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
public class InvoicePosition
{
    public int InvoicePositionId { get; set; }
    public int InvoiceId { get; set; }    
    public int? ProductId { get; set; }
    public Product Product { get; set; }
    public string ProductName { get; set; }
    public string ProductDescription { get; set; }
    public decimal? ProductValue { get; set; }
    public int Quantity { get; set; }

    public string VatRate { get; set; } = "23%";
    public decimal GetNetValue() => Math.Round((ProductValue ?? 0) * Quantity, 2, MidpointRounding.AwayFromZero);    

    public decimal GetVatValue()
    {
        if (string.IsNullOrWhiteSpace(VatRate) || VatRate.ToLower() == "zw")
            return 0;

        
        var cleanRate = VatRate.Replace("%", "").Replace(",", ".");

        if (decimal.TryParse(cleanRate, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var rate))
        {
            var vat = GetNetValue() * (rate / 100);
            return Math.Round(vat, 2, MidpointRounding.AwayFromZero);
        }

        return 0;
    }

    public decimal GetGrossValue() => GetNetValue() + GetVatValue();
}