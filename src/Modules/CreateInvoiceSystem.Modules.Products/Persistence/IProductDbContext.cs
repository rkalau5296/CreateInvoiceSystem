using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Products.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Modules.Products.Persistence;

public interface IProductDbContext : ISaveChangesContext
{
    public DbSet<Product> Products { get; set; }
}
