namespace CreateInvoiceSystem.Invoices.Persistence.Shared.Entities;

public class InvoicePositionEntity
{
    public int InvoicePositionId { get; set; }
    public int InvoiceId { get; set; }    
    public int? ProductId { get; set; }    
    public string ProductName { get; set; } = string.Empty;
    public string? ProductDescription { get; set; }
    public decimal? ProductValue { get; set; }
    public int Quantity { get; set; }
    public string VatRate { get; set; } = "23%";
    public InvoiceEntity Invoice { get; set; } = null!;
}