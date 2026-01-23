namespace CreateInvoiceSystem.Modules.Invoices.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string CompanyName { get; set; }
    public string Nip { get; set; }
    public Address Address { get; set; }
    public int AddressId { get; set; }
    public string BankAccountNumber { get; set; } = string.Empty;

}
