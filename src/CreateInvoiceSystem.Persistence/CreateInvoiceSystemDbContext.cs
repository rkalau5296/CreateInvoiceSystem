using CreateInvoiceSystem.Modules.Clients.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Entities;
using CreateInvoiceSystem.Modules.Addresses.Persistence;
using CreateInvoiceSystem.Modules.InvoicePositions.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence;
using CreateInvoiceSystem.Modules.Invoices.Entities;
using CreateInvoiceSystem.Modules.Invoices.Persisstence;
using CreateInvoiceSystem.Modules.Products.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence;
using CreateInvoiceSystem.Modules.Users.Persistence;
using Microsoft.EntityFrameworkCore;
using CreateInvoiceSystem.Modules.Users.Entities;


namespace CreateInvoiceSystem.Persistence;

public class CreateInvoiceSystemDbContext(DbContextOptions<CreateInvoiceSystemDbContext> options) : 
    DbContext(options), IClientDbContext, IAddressDbContext, IProductDbContext, IInvoicePosistionDbContext, IInvoiceDbContext, IUserDbContext, IDbContext
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<Product> Products { get; set; }    
    public DbSet<InvoicePosition> InvoicePositions { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Address> Addresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {        
        modelBuilder.Entity<User>()
            .HasOne(u => u.Address)
            .WithMany()
            .HasForeignKey(u => u.AddressId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Invoices)
            .WithOne()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Clients)
            .WithOne()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Products)
            .WithOne()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Nip)
            .IsUnique();
        
        modelBuilder.Entity<Client>()
            .HasOne(c => c.Address)
            .WithMany()
            .HasForeignKey(c => c.AddressId)
            .OnDelete(DeleteBehavior.NoAction);
        
        modelBuilder.Entity<Client>()
            .HasIndex(c => c.Nip)
            .IsUnique();
        
        modelBuilder.Entity<Invoice>()
            .Ignore(i => i.ClientId);

        modelBuilder.Entity<Invoice>()
            .Ignore(i => i.Client);

        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.InvoicePositions)
            .WithOne()
            .HasForeignKey(pos => pos.InvoiceId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.PaymentDate)
            .HasColumnType("date");

        modelBuilder.Entity<Invoice>()
            .Property(i => i.CreatedDate)
            .HasColumnType("date");

        modelBuilder.Entity<InvoicePosition>()
            .Ignore(ip => ip.ProductId);
        modelBuilder.Entity<InvoicePosition>()
            .Ignore(ip => ip.Product);

        base.OnModelCreating(modelBuilder);
    }
}

