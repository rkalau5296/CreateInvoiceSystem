namespace CreateInvoiceSystem.Persistence;

using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

public class CreateInvoiceSystemDbContext(DbContextOptions<CreateInvoiceSystemDbContext> options) : DbContext(options), IDbContext
{
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoicePosition> InvoicePositions => Set<InvoicePosition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne(u => u.Address)
            .WithMany()
            .HasForeignKey(u => u.AddressId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<User>()
            .HasMany(u => u.Invoices)
            .WithOne(i => i.User)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<User>()
            .HasMany(u => u.Clients)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<User>()
            .HasMany(u => u.Products)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Client>()
            .HasOne(c => c.Address)
            .WithMany()
            .HasForeignKey(c => c.AddressId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Client>()
            .HasIndex(c => c.Nip)
            .IsUnique();
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Client)
            .WithMany()
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.User)
            .WithMany(u => u.Invoices)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.InvoicePositions)
            .WithOne(pos => pos.Invoice)
            .HasForeignKey(pos => pos.InvoiceId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<InvoicePosition>()
            .HasOne(ip => ip.Product)
            .WithMany()
            .HasForeignKey(ip => ip.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}

