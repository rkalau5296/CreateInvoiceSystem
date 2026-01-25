namespace CreateInvoiceSystem.Frontend.Models;

public class InvoicePositionDto
{
    public int InvoiceId { get; set; }
    public int? ProductId { get; set; }
    public int Quantity { get; set; } = 1;    
    public string? ProductName { get; set; }
    public decimal? ProductValue { get; set; }    
    public ProductDto Product { get; set; } = new();
    public string ProductDescription { get; set; } = string.Empty;
    public decimal TotalValue => Quantity * (ProductId > 0
        ? (Product.Value ?? 0)
        : (ProductValue ?? 0));
    public string VatRate { get; set; } = "23%";
}
