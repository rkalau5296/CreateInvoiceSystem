namespace CreateInvoiceSystem.Modules.Clients.Domain.Entities;
public class Client 
{
    public int ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Nip { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int AddressId { get; set; }
    public Address Address { get; set; } = null!;
    public int UserId { get; set; }

}
