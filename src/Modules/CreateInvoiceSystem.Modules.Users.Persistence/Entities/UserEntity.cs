using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreateInvoiceSystem.Modules.Users.Persistence.Entities;

public class UserEntity : IdentityUser<int>
{   
    public string Name { get; set; }   
    public string CompanyName { get; set; }
    public string Nip { get; set; }    
    public int AddressId { get; set; }

    [NotMapped]
    public IEnumerable<object> Invoices { get; set; } = [];
    [NotMapped]
    public IEnumerable<object> Products { get; set; } = [];
    [NotMapped]
    public IEnumerable<object> Clients { get; set; } = [];
}
