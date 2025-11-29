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
    public DbSet<InvoicePosition> InvoicePosition => Set<InvoicePosition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // USER - ADDRESS (jeden address na usera)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Address)
            .WithMany()
            .HasForeignKey(u => u.AddressId)
            .OnDelete(DeleteBehavior.NoAction);

        // USER - INVOICES (wiele faktur na usera)
        modelBuilder.Entity<User>()
            .HasMany(u => u.Invoices)
            .WithOne(i => i.User)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // USER - CLIENTS (wiele klientów na usera)
        modelBuilder.Entity<User>()
            .HasMany(u => u.Clients)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // USER - PRODUCTS (wiele produktów na usera)
        modelBuilder.Entity<User>()
            .HasMany(u => u.Products)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // CLIENT - ADDRESS (jeden address na klienta)
        modelBuilder.Entity<Client>()
            .HasOne(c => c.Address)
            .WithMany()
            .HasForeignKey(c => c.AddressId)
            .OnDelete(DeleteBehavior.NoAction);

        // INVOICE - CLIENT (wiele faktur na klienta, nawigacja JEDNOSTRONNA)
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Client)
            .WithMany()                  // po stronie Client BRAK kolekcji Invoices!
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.NoAction);

        // INVOICE - USER (user wystawia fakturę)
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.User)
            .WithMany(u => u.Invoices)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // INVOICE - INVOICEPOSITIONS
        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.InvoicePositions)
            .WithOne(pos => pos.Invoice)
            .HasForeignKey(pos => pos.InvoiceId)
            .OnDelete(DeleteBehavior.NoAction);

        // PRODUCT - INVOICEPOSITIONS
        modelBuilder.Entity<InvoicePosition>()
            .HasOne(ip => ip.Product)
            .WithMany() 
            .HasForeignKey(ip => ip.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}

