namespace CreateInvoiceSystem.Modules.Users.Domain.Entities;

public class User
{   
    public int UserId { get; set; }
    public string Name { get; set; }   
    public string CompanyName { get; set; }
    public string Email { get; set; }    
    public string Password { get; set; }
    public string Nip { get; set; }    
    public Address Address { get; set; }
    public int AddressId { get; set; }
    public string? BankAccountNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Invoice> Invoices { get; set; } = [];    
    public ICollection<Client> Clients { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
