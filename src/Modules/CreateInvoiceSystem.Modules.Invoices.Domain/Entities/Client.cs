namespace CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
public class Client 
{  
    public int ClientId { get; set; }
    public string Name { get; set; }
    public string Nip { get; set; }
    public int AddressId { get; set; }    
    public Address Address { get; set; }    
    public int UserId { get; set; }    
    public bool IsDeleted { get; set; }
}
