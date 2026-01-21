namespace CreateInvoiceSystem.Frontend.Models;

public class ClientDto
{
    public string Name { get; set; } = string.Empty;
    public string Nip { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = new();
    public int UserId { get; set; }
    public bool IsDeleted { get; set; } = false;
}
