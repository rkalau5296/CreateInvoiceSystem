namespace CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
public record CreateInvoiceDto
{
    public string Title { get; set; }
    public string Comments { get; set; }
    public string MethodOfPayment { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalGross { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; }
    public int? ClientId { get; set; }
    public CreateClientDto Client { get; set; }
    public string SellerName { get; set; }
    public string SellerNip { get; set; }
    public string SellerAddress { get; set; }
    public string BankAccountNumber { get; set; }
    public string ClientName { get; set; }
    public string ClientAddress { get; set; }
    public string ClientNip { get; set; }
    public List<InvoicePositionDto> InvoicePositions { get; set; }
}