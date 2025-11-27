namespace CreateInvoiceSystem.Abstractions.Entities;

public class Client 
{
    public Client()
    {
        Invoices = [];
    }
    public int ClientId { get; set; }
    public string Name { get; set; }
    public string Nip { get; set; }
    public int AddressId { get; set; }    
    public Address Address { get; set; }
    public ICollection<Invoice> Invoices { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}
