namespace CreateInvoiceSystem.Modules.Products.Entities;

public class Product
{
    public int ProductId { get; set; }    
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal? Value { get; set; }
    public int UserId { get; set; }    
    public bool IsDeleted { get; set; }
}
