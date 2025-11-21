namespace CreateInvoiceSystem.Abstractions.EntitiesBases;

public abstract class ClientBase
{
    public int ClientId { get; set; }
    public string Name { get; set; }
    public int AddressId { get; set; }
    public string Email { get; set; }
    public int UserId { get; set; }
}
