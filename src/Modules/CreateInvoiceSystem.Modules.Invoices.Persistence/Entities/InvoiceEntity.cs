namespace CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;

public class InvoiceEntity
{    
    public int InvoiceId { get; set; }    
    public string Title { get; set; }
    public decimal TotalAmount { get; set; }        
    public DateTime PaymentDate { get; set; }    
    public DateTime CreatedDate { get; set; }    
    public string Comments { get; set; }    
    public int? ClientId { get; set; }    
    public int UserId { get; set; }    
    public string MethodOfPayment { get; set; }
    public string ClientName { get; set; }
    public string ClientAddress { get; set; }
    public string ClientNip { get; set; }    
}
