namespace CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;

public class InvoicePositionEntity
{
    public int InvoicePositionId { get; set; }
    public int InvoiceId { get; set; }    
    public int? ProductId { get; set; }    
    public string ProductName { get; set; }
    public string ProductDescription { get; set; }
    public decimal? ProductValue { get; set; }
    public int Quantity { get; set; }
    public string VatRate { get; set; } = "23%";
}