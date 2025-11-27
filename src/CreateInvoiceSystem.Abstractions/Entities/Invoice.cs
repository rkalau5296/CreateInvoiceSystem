namespace CreateInvoiceSystem.Abstractions.Entities;

public class Invoice
{
    public Invoice()
    {        
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
    public Client Client { get; set; }
    public User User { get; set; }
    public string MethodOfPayment { get; set; }        
    public ICollection<Product> Products { get; set; }
    public string Product { get; set; }
}
