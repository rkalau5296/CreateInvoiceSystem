namespace CreateInvoiceSystem.Frontend.Models
{
    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string MethodOfPayment { get; set; } = "Przelew";
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Today.AddDays(14);
        public DateTime CreatedDate { get; set; } = DateTime.Today;
        public int UserId { get; set; }
        public string? UserEmail { get; set; }        
        public int? ClientId { get; set; }        
        public string? ClientName { get; set; }
        public string? ClientNip { get; set; }
        public string? ClientAddress { get; set; }        
        public ClientDto? Client { get; set; }
        public List<InvoicePositionDto> InvoicePositions { get; set; } = new();
    }
}
