namespace CreateInvoiceSystem.Modules.Users.Domain.Entities;

public class Address 
{   
    public int AddressId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; }= string.Empty;
    public string Country { get; set; } = string.Empty;

}
