namespace CreateInvoiceSystem.Modules.Products.Entities;

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Value { get; set; }
    public int UserId { get; set; }
}
