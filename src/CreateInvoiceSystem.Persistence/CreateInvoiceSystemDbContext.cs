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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CreateInvoiceSystemDbContext).Assembly);        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClientEntityConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AddressEntityConfiguration).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductEntityConfiguration).Assembly);  

        var user = modelBuilder.Entity<UserEntity>();

        modelBuilder.Entity<UserEntity>()
        .HasIndex(u => u.Nip)
        .IsUnique();

        user.Property(u => u.Id).HasColumnName("UserId");
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
                .OnDelete(DeleteBehavior.SetNull);
        
        position.HasIndex(p => new { p.InvoiceId, p.ProductId });

        var invoice = modelBuilder.Entity<InvoiceEntity>();
        invoice.HasKey(i => i.InvoiceId);
        invoice.Property(i => i.InvoiceId).ValueGeneratedOnAdd();

        invoice.Ignore(i => i.InvoicePositions);

        invoice.HasOne<ClientEntity>()
               .WithMany()
               .HasForeignKey(i => i.ClientId)
               .OnDelete(DeleteBehavior.SetNull);

        var session = modelBuilder.Entity<UserSessionEntity>();
        session.HasKey(s => s.Id);
        session.Property(s => s.Id).ValueGeneratedOnAdd();
        session.Property(s => s.RefreshToken).IsRequired();
        
        session.HasOne<UserEntity>()
               .WithMany()
               .HasForeignKey(s => s.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        
        session.HasIndex(s => s.RefreshToken).IsUnique();

        modelBuilder.ApplyConfiguration(new AddressEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ClientEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProductEntityConfiguration());        
    }
}