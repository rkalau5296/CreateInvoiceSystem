namespace CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
public class Invoice
{
    public int InvoiceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Comments { get; set; } = string.Empty;
    public int? ClientId { get; set; }
    public int UserId { get; set; }
    public Client Client { get; set; } = null!;
    public string MethodOfPayment { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public string SellerNip { get; set; } = string.Empty;
    public string SellerAddress { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientAddress { get; set; } = string.Empty;
    public string ClientNip { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
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
