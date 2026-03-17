namespace CreateInvoiceSystem.Frontend.Models
{
    public class UserDtoData
    {
        public int UserId { get; set; }
        public string Name { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Nip { get; set; } = "";
        public string BankAccountNumber { get; set; } = "";
        public UpdateAddressDto Address { get; set; } = new();
    }
}
