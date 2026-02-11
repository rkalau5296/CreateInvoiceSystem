using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Frontend.Models;

public class ProductDto
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Nazwa produktu jest wymagana")]
    [StringLength(100, ErrorMessage = "Nazwa nie może przekraczać 100 znaków")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Opis nie może przekraczać 500 znaków")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wartość jest wymagana")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Wartość musi być większa niż 0")]
    public decimal? Value { get; set; }

    public int UserId { get; set; }

    public bool IsDeleted { get; set; }
}
