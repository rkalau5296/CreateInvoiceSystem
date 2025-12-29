namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;
public record CreateInvoiceDto
{
    public string Title { get; set; }
    public string Comments { get; set; }
    public string MethodOfPayment { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public int UserId { get; set; }
    public int? ClientId { get; set; }
    public CreateClientDto Client { get; set; }
    public string ClientName { get; set; }
    public string ClientAddress { get; set; }
    public string ClientNip { get; set; }
    public List<InvoicePositionDto> InvoicePositions { get; set; }
}