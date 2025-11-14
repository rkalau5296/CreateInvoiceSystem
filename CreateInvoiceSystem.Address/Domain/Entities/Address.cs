namespace CreateInvoiceSystem.Address.Domain.Entities;

using CreateInvoiceSystem.SharedKernel.Domain;
using System.ComponentModel.DataAnnotations;

public class Address(int addressId, string street, string number, string city, string postalCode, string email)
{
    //public Address()
    //{
    //    Clients = [];
    //}
    public int AddressId { get; set; } = addressId;

    [Required]
    [Display(Name = "Ulica")]
    [MaxLength(255)]
    public string Street { get; set; } = street;
    [Required]
    [Display(Name = "Numer")]
    [MaxLength(255)]
    public string Number { get; set; } = number;
    [Required]
    [Display(Name = "Miejscowość")]
    [MaxLength(255)]
    public string City { get; set; } = city;
    [Required]
    [Display(Name = "kod pocztowy")]
    [MaxLength(255)]
    public string PostalCode { get; set; } = postalCode;
    public string Email { get; set; } = email;
    //public ICollection<Client> Clients { get; set; }
}
