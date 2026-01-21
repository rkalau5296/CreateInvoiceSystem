namespace CreateInvoiceSystem.Frontend.Models;

public class InvoicePositionDto
{
    public int? ProductId { get; set; }
    public int Quantity { get; set; }
    public ProductDto? Product { get; set; }    
    public string? ProductName { get; set; }
    public decimal? ProductValue { get; set; }
}
