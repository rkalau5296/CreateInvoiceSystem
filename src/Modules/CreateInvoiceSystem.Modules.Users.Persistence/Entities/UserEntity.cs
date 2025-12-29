namespace CreateInvoiceSystem.Modules.Users.Persistence.Entities;

public class UserEntity
{   
    public int UserId { get; set; }
    public string Name { get; set; }   
    public string CompanyName { get; set; }
    public string Email { get; set; }    
    public string Password { get; set; }
    public string Nip { get; set; }    
    public int AddressId { get; set; }
}
