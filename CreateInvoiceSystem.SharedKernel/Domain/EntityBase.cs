﻿namespace CreateInvoiceSystem.SharedKernel.Domain;

using System.ComponentModel.DataAnnotations;
public abstract class EntityBase
{
    [Key]
    public int Id { get; set; }
}
