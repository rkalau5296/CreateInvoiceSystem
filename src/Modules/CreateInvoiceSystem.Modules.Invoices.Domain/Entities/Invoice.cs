namespace CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
public class Invoice
{
    public int InvoiceId { get; set; }
    public string Title { get; set; }    
    public DateTime PaymentDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Comments { get; set; }
    public int? ClientId { get; set; }
    public int UserId { get; set; }
    public Client Client { get; set; }
    public string MethodOfPayment { get; set; }
    public string SellerName { get; set; }
    public string SellerNip { get; set; }
    public string SellerAddress { get; set; }
    public string BankAccountNumber { get; set; }
    public string ClientName { get; set; }
    public string ClientAddress { get; set; }
    public string ClientNip { get; set; }
    public ICollection<InvoicePosition> InvoicePositions { get; set; } = [];

    public decimal TotalNet { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalGross { get; set; }

    public void RecalculateTotals()
    {
        TotalNet = InvoicePositions.Sum(p => p.GetNetValue());
        TotalVat = InvoicePositions.Sum(p => p.GetVatValue());
        TotalGross = TotalNet + TotalVat;
    }
}
