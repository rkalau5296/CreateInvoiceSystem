namespace CreateInvoiceSystem.Persistence;

using CreateInvoiceSystem.Abstractions.DbContext;
using Microsoft.EntityFrameworkCore;
using AddressEntity = Address.Domain.Entities.Address;

public class CreateInvoiceSystemDbContext(DbContextOptions<CreateInvoiceSystemDbContext> options) : DbContext(options), IDbContext
{
    public DbSet<AddressEntity> Addresses => Set<AddressEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CreateInvoiceSystemDbContext).Assembly);

        modelBuilder.Entity<AddressEntity>().ToTable("Addresses");
    }
}
