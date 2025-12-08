using CreateInvoiceSystem.Abstractions.Entities;

namespace CreateInvoiceSystem.Modules.Clients.Entities;

public class Client 
{  
    public int ClientId { get; set; }
    public string Name { get; set; }
    public string Nip { get; set; }
    public int AddressId { get; set; }    
    public Address Address { get; set; }    
    public int UserId { get; set; }
    public User User { get; set; }
    public bool IsDeleted { get; set; }
}
