namespace CreateInvoiceSystem.Shared.Persistence;

public class AddressEntity 
{   
    public int AddressId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;    
    public string Country { get; set; } = string.Empty;    
}