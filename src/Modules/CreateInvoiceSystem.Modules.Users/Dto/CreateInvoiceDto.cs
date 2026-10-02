namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;
public record CreateInvoiceDto
{
    public string Title { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public string MethodOfPayment { get; set; } = string.Empty;
    public decimal TotalNet { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalGross { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public int UserId { get; set; }
    public int? ClientId { get; set; }
    public CreateClientDto Client { get; set; } = null!;
    public string ClientName { get; set; } = string.Empty;
    public string ClientAddress { get; set; } = string.Empty;
    public string ClientNip { get; set; } = string.Empty;
    public string? ClientEmail { get; set; } 
    public List<InvoicePositionDto> InvoicePositions { get; set; } = new List<InvoicePositionDto>();
}