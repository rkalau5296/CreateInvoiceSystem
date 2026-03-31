using CreateInvoiceSystem.Frontend.Validators;
using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Frontend.Models;

public class InvoicePositionDto
{
    public int InvoiceId { get; set; }

    public int? ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Ilość musi wynosić co najmniej 1")]
    public int Quantity { get; set; } = 1;

    [Required(ErrorMessage = "Nazwa produktu jest wymagana")]
    public string? ProductName { get; set; }

    [Required(ErrorMessage = "Cena produktu jest wymagana")]
    [MinDecimal("0.01", ErrorMessage = "Cena musi być większa niż 0")]
    [MaxDecimal("99999999999999999999.99", ErrorMessage = "Cena jest za duża")]
    public decimal? ProductValue { get; set; }

    public ProductDto Product { get; set; } = new();

    public string ProductDescription { get; set; } = string.Empty;

    public decimal TotalValue => Quantity * (ProductId > 0
        ? (Product.Value ?? 0)
        : (ProductValue ?? 0));

    [Required(ErrorMessage = "Stawka VAT jest wymagana")]
    public string VatRate { get; set; } = "23%";
}