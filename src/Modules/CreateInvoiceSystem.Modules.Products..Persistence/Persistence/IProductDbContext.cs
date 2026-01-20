using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Products.Persistence.Persistence;

public interface IProductDbContext : ISaveChangesContext
{
    DbSet<ProductEntity> Products { get; set; }
}
