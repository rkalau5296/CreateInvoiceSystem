namespace CreateInvoiceSystem.Modules.Invoices.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Nip { get; set; } = string.Empty;
    public Address Address { get; set; } = null!;
    public int AddressId { get; set; }
    public string BankAccountNumber { get; set; } = string.Empty;

}
