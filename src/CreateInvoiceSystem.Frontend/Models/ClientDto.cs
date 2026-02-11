using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Frontend.Models;

public class ClientDto
{
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Musisz podać nazwę klienta")]
    [MinLength(2, ErrorMessage = "Nazwa jest za krótka")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "NIP jest obowiązkowy")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "NIP musi mieć dokładnie 10 cyfr")]
    public string Nip { get; set; } = string.Empty;    
    public AddressDto Address { get; set; } = new();
    public int AddressId { get; set; }
    public int UserId { get; set; }
    public bool IsDeleted { get; set; }
}
