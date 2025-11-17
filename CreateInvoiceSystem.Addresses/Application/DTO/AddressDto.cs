using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Addresses.Application.DTO;

public record AddressDto(
    int AddressId,

    [Required]
    [Display(Name = "Ulica")]
    [MaxLength(255)]
    string Street,

    [Required]
    [Display(Name = "Numer")]
    [MaxLength(255)]
    string Number,

    [Required]
    [Display(Name = "Miejscowość")]
    [MaxLength(255)]
    string City,

    [Required]
    [Display(Name = "kod pocztowy")]
    [MaxLength(255)]
    string PostalCode,
    string Email
);
