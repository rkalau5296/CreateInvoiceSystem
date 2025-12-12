using CreateInvoiceSystem.Modules.Products.Entities;

namespace CreateInvoiceSystem.Modules.InvoicePositions.Entities;

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
}