namespace CreateInvoiceSystem.Frontend.Models;

public class ClientDto
{
    public int ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Nip { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = new AddressDto();
    public int AddressId { get; set; }
    public int UserId { get; set; }
}