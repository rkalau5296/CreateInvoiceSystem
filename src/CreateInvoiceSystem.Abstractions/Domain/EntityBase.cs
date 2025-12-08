using System.ComponentModel.DataAnnotations;

namespace CreateInvoiceSystem.Abstractions.Domain;

public abstract class EntityBase
{
    [Key]
    public int Id { get; set; }
}
