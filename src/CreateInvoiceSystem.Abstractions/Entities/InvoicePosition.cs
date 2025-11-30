namespace CreateInvoiceSystem.Abstractions.Entities;

public class InvoicePosition
{
    public int InvoicePositionId { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; }
    public int? ProductId { get; set; }
    public Product Product { get; set; }
    public string Description { get; set; }
    public string Name { get; set; }
    public decimal? Value { get; set; }
    public int Quantity { get; set; }
}