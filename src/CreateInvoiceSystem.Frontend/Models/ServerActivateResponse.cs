namespace CreateInvoiceSystem.Frontend.Models
{
    public class ServerActivateResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Detail { get; set; }
        public string? Title { get; set; }
    }
}
