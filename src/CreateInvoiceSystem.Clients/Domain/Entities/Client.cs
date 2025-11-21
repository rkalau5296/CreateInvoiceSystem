namespace CreateInvoiceSystem.Clients.Domain.Entities;

using CreateInvoiceSystem.Abstractions.EntitiesBases;

public class Client : ClientBase
{
    //public Client()
    //{
    //    Invoices = [];
    //}

   
    public AddressBase Address { get; set; }
    //public ICollection<Invoice> Invoices { get; set; }
    //public User User { get; set; }
}
