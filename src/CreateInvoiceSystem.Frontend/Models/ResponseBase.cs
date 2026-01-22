namespace CreateInvoiceSystem.Frontend.Models
{
    public class ResponseBase<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
