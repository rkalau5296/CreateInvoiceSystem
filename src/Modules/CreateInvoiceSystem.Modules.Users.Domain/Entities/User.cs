namespace CreateInvoiceSystem.Modules.Users.Domain.Entities;

public class User
{   
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nip { get; set; } = string.Empty;
    public Address Address { get; set; } = null!;
    public int AddressId { get; set; }
    public string? BankAccountNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Invoice> Invoices { get; set; } = [];    
    public ICollection<Client> Clients { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
