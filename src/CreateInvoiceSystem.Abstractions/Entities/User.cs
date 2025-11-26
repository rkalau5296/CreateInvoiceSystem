namespace CreateInvoiceSystem.Abstractions.Entities;

public class User
{
    public User()
    {
        //Invoices = [];
        Clients = [];
        Products = [];
        //MethodOfPayments = [];
    }
    
    public int UserId { get; set; }
    public string Name { get; set; }    
    public string Email { get; set; }    
    public string Password { get; set; }
    public string Nip { get; set; }

    //public ICollection<Invoice> Invoices { get; set; }
    public ICollection<Client> Clients { get; set; }
    public ICollection<Product> Products { get; set; }
    //public ICollection<MethodOfPayment> MethodOfPayments { get; set; }
}
