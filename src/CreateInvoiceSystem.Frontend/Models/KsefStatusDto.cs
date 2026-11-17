namespace CreateInvoiceSystem.Frontend.Models
{
    public class KsefStatusDto
    {
        public int Status { get; set; }
        public string? KsefReferenceNumber { get; set; }
        public string? ErrorMessage { get; set; }
    }
}