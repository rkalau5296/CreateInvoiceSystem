using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Configuration;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Clients.Persistence.Configuration;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Persistence;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Configuration;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Products.Persistence.Configuration;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Users.Persistence.Configuration;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.Persistence;

public class CreateInvoiceSystemDbContext(DbContextOptions<CreateInvoiceSystemDbContext> options)
    : IdentityDbContext<UserEntity, IdentityRole<int>, int>(options),
      IAddressDbContext, IClientDbContext, IProductDbContext, IInvoicePosistionDbContext, IInvoiceDbContext, IUserDbContext, IDbContext
{
    public DbSet<AddressEntity> Addresses { get; set; }
    public DbSet<ClientEntity> Clients { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<InvoicePositionEntity> InvoicePositions { get; set; }
    public DbSet<InvoiceEntity> Invoices { get; set; }
    public DbSet<UserSessionEntity> UserSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AddressEntityConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClientEntityConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductEntityConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserEntityConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvoiceEntityConfiguration).Assembly);

        modelBuilder.Entity<UserEntity>()
            .HasOne<AddressEntity>()
            .WithMany()
            .HasForeignKey(u => u.AddressId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<InvoiceEntity>()
            .HasOne<ClientEntity>()
            .WithMany()
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<InvoiceEntity>()
            .HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<InvoicePositionEntity>()
            .HasKey(p => p.InvoicePositionId);

        modelBuilder.Entity<InvoicePositionEntity>()
            .HasOne<InvoiceEntity>()
            .WithMany()
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<InvoicePositionEntity>()
            .HasOne<ProductEntity>()
            .WithMany()
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<InvoicePositionEntity>()
            .Property(p => p.ProductValue)
            .HasPrecision(38, 2);
    }
}