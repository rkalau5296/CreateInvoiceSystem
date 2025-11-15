namespace CreateInvoiceSystem.Address.Domain.Entities;

using CreateInvoiceSystem.SharedKernel.Domain;
using System.ComponentModel.DataAnnotations;

public class Address(int addressId, string street, string number, string city, string postalCode, string email)
{
    //public Address()
    //{
    //    Clients = [];
    //}

    public int AddressId { get; set; } = addressId;

    
    public string Street { get; set; } = street;
    
    public string Number { get; set; } = number;
    
    public string City { get; set; } = city;
    
    public string PostalCode { get; set; } = postalCode;
    public string Email { get; set; } = email;
    //public ICollection<Client> Clients { get; set; }
}
