namespace CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;

public class InvoiceEntity
{    
    public int InvoiceId { get; set; }    
    public string Title { get; set; }    
    public DateTime PaymentDate { get; set; }    
    public DateTime CreatedDate { get; set; }    
    public string Comments { get; set; }    
    public int? ClientId { get; set; }    
    public int UserId { get; set; }    
    public string MethodOfPayment { get; set; }
    public string SellerName { get; set; }
    public string SellerNip { get; set; }
    public string SellerAddress { get; set; }
    public string BankAccountNumber { get; set; }
    public string ClientName { get; set; }
    public string ClientAddress { get; set; }
    public string ClientNip { get; set; }
    public IEnumerable<object> InvoicePositions { get; set; } = [];

    public decimal TotalNet { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalGross { get; set; }
}
