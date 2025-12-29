namespace CreateInvoiceSystem.Modules.Products.Persistence.Entities;

public class ProductEntity
{
    public int ProductId { get; set; }    
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal? Value { get; set; }
    public int UserId { get; set; }    
    public bool IsDeleted { get; set; }
}
