namespace CreateInvoiceSystem.Addresses.Domain.Entities;

public class Address()
{   
    //public Address()
    //{
    //    Clients = [];
    //}

    public int AddressId { get; set; }
    
    public string Street { get; set; }
    
    public string Number { get; set; }
    
    public string City { get; set; }
    
    public string PostalCode { get; set; }

    public string Email { get; set; }

    public string Country { get; set; }
    //public ICollection<Client> Clients { get; set; }
}
