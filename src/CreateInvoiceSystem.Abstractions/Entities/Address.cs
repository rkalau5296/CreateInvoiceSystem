namespace CreateInvoiceSystem.Abstractions.Entities;

public class Address 
{   
    public int AddressId { get; set; }
    public string Street { get; set; }
    public string Number { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }    
    public string Country { get; set; }
    public int UserId { get; set; }    
}
