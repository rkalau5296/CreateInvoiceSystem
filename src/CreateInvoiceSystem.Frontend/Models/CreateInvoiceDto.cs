namespace CreateInvoiceSystem.Frontend.Models
{
    public class CreateInvoiceDto
    {
        public string Title { get; set; } = "";
        public string Comments { get; set; } = "";
        public string MethodOfPayment { get; set; } = "Przelew";
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now.AddDays(14);
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }        
        public int? ClientId { get; set; }
        public ClientDto? Client { get; set; }
        public List<InvoicePositionDto> InvoicePositions { get; set; } = new();
    }
}
