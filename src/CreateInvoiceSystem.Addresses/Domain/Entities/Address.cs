namespace CreateInvoiceSystem.Addresses.Domain.Entities;

using CreateInvoiceSystem.Abstractions.EntitiesBases;

public class Address : AddressBase
{
    public Address()
    {
        Clients = [];
    } 
    public ICollection<ClientBase> Clients { get; set; }
}
