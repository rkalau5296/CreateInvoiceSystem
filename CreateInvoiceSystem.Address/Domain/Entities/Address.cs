namespace CreateInvoiceSystem.Address.Domain.Entities;

using CreateInvoiceSystem.SharedKernel.Domain;
using System.ComponentModel.DataAnnotations;

public class Address : EntityBase
{
    //public Address()
    //{
    //    Clients = [];
    //}

    [Required]
    [Display(Name = "Ulica")]
    [MaxLength(255)]
    public string Street { get; set; }
    [Required]
    [Display(Name = "Numer")]
    [MaxLength(255)]
    public string Number { get; set; }
    [Required]
    [Display(Name = "Miejscowość")]
    [MaxLength(255)]
    public string City { get; set; }
    [Required]
    [Display(Name = "kod pocztowy")]
    [MaxLength(255)]
    public string PostalCode { get; set; }
    public string Email { get; set; }
    //public ICollection<Client> Clients { get; set; }
}
