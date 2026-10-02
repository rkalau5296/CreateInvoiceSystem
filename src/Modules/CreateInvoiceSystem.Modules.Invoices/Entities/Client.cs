namespace CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
public class Client 
{  
    public int ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Nip { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Address Address { get; set; } = null!;
    public int AddressId { get; set; }        
    public int UserId { get; set; }        
}
