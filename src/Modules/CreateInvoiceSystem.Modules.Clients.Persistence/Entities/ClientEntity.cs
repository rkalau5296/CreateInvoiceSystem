namespace CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
public class ClientEntity
{
    public int ClientId { get; set; }
    public string Name { get; set; }
    public string Nip { get; set; }
    public int AddressId { get; set; }    
    public int UserId { get; set; }
    public bool IsDeleted { get; set; }
}
