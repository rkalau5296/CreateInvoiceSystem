namespace CreateInvoiceSystem.Modules.Users.Domain.Entities;
public class InvoicePosition
{
    public int InvoicePositionId { get; set; }
    public int InvoiceId { get; set; }    
    public int? ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public decimal? ProductValue { get; set; }
    public int Quantity { get; set; }
    public string VatRate { get; set; } = "23%";
}