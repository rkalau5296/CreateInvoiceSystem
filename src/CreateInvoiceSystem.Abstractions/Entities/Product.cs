namespace CreateInvoiceSystem.Abstractions.Entities;

public class Product
{
    public Product()
    {
        //InvoicePositions = [];
        Invoices = [];
    }

    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public ICollection<Invoice> Invoices { get; set; }
    //public ICollection<InvoicePosition> InvoicePositions { get; set; }
}
