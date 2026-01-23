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
    public decimal GetNetValue() => (ProductValue ?? 0) * Quantity;
    public decimal GetVatValue()
    {
        if (VatRate == "zw") return 0;
        
        if (decimal.TryParse(VatRate.Replace("%", ""), out var rate))
        {
            return GetNetValue() * (rate / 100);
        }
        return 0;
    }

    public decimal GetGrossValue() => GetNetValue() + GetVatValue();
}