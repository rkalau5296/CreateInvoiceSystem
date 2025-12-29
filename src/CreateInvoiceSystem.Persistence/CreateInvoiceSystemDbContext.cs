using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Configuration;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Clients.Persistence.Configuration;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Persistence;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Products.Persistence.Configuration;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Persistence;

public class CreateInvoiceSystemDbContext(DbContextOptions<CreateInvoiceSystemDbContext> options) : DbContext(options), IAddressDbContext, IClientDbContext, IProductDbContext, IInvoicePosistionDbContext, IInvoiceDbContext, IUserDbContext, IDbContext
{
    public DbSet<AddressEntity> Addresses { get; set; }
    public DbSet<ClientEntity> Clients { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<InvoicePositionEntity> InvoicePositions { get; set; }
    public DbSet<InvoiceEntity> Invoices { get; set; }
    public DbSet<UserEntity> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {        
        var user = modelBuilder.Entity<UserEntity>();
        user.HasKey(u => u.UserId);
        user.Property(u => u.UserId).ValueGeneratedOnAdd();
        user.HasOne<AddressEntity>()
            .WithMany()
            .HasForeignKey(u => u.AddressId)
            .OnDelete(DeleteBehavior.NoAction);
        
        user.HasMany<InvoiceEntity>()
            .WithOne()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        user.HasMany<ClientEntity>()
            .WithOne()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        user.HasMany<ProductEntity>()
            .WithOne()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        
        var client = modelBuilder.Entity<ClientEntity>();
        client.HasOne<AddressEntity>()
              .WithOne()
              .HasForeignKey<ClientEntity>(c => c.AddressId)
              .OnDelete(DeleteBehavior.NoAction);
        
        var position = modelBuilder.Entity<InvoicePositionEntity>();
        position.HasKey(p => p.InvoicePositionId);
        position.Property(p => p.InvoicePositionId).ValueGeneratedOnAdd();
        position.HasIndex(p => new { p.InvoiceId, p.ProductId });

        position.HasOne<InvoiceEntity>()
                .WithMany() 
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.NoAction);

        position.HasOne<ProductEntity>()
                .WithMany() 
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.NoAction);
        
        position.HasIndex(p => new { p.InvoiceId, p.ProductId });

        var invoice = modelBuilder.Entity<InvoiceEntity>();
        invoice.HasKey(i => i.InvoiceId);
        invoice.Property(i => i.InvoiceId).ValueGeneratedOnAdd();

        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AddressEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ClientEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProductEntityConfiguration());        
    }
}