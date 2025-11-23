namespace CreateInvoiceSystem.Persistence;

using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

public class CreateInvoiceSystemDbContext(DbContextOptions<CreateInvoiceSystemDbContext> options) : DbContext(options), IDbContext
{
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Client> Clients => Set<Client>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CreateInvoiceSystemDbContext).Assembly);

        modelBuilder.Entity<Client>()
            .HasOne(c => c.Address)
            .WithMany(a => a.Clients)
            .HasForeignKey(c => c.AddressId);

        modelBuilder.Entity<Address>().ToTable("Addresses");
        modelBuilder.Entity<Client>().ToTable("Clients");
    }
}
