namespace CreateInvoiceSystem.Frontend.Models
{
    public class UpdateUserDto
    {
        public int UserId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string CompanyName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;        
        public string Nip { get; init; } = string.Empty;
        public string BankAccountNumber { get; init; } = string.Empty;
        public UpdateAddressDto Address { get; init; } = new();
    }
}
