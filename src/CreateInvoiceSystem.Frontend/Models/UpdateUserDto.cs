namespace CreateInvoiceSystem.Frontend.Models
{
    public class UpdateUserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Nip { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public UpdateAddressDto Address { get; set; } = new();
    }
}