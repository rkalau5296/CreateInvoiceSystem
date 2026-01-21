namespace CreateInvoiceSystem.Frontend.Models;

public class ProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Value { get; set; }
    public int UserId { get; set; }
    public bool IsDeleted { get; set; }
};
