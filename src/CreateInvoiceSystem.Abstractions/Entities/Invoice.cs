namespace CreateInvoiceSystem.Abstractions.Entities;

public class Invoice
{
    public Invoice()
    {
        //InvoicePositions = new Collection<InvoicePosition>();
        Products = [];
    }
    public int InvoiceId { get; set; }    
    public string Title { get; set; }
    public decimal Value { get; set; }        
    public DateTime PaymentDate { get; set; }    
    public DateTime CreatedDate { get; set; }    
    public string Comments { get; set; }    
    public int ClientId { get; set; }    
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }    
    public Client Client { get; set; }
    public User User { get; set; }
    //public int MethodOfPaymentId { get; set; }    
    //public MethodOfPayment MethodOfPayment { get; set; }
    //public ICollection<InvoicePosition> InvoicePositions { get; set; }
    public ICollection<Product> Products { get; set; }
}
