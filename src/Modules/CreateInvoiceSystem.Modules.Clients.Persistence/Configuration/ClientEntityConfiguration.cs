using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreateInvoiceSystem.Modules.Clients.Persistence.Configuration;

public class ClientEntityConfiguration : IEntityTypeConfiguration<ClientEntity>
{
    public void Configure(EntityTypeBuilder<ClientEntity> builder)
    {        
        builder.ToTable("Clients");
        builder.HasKey(c => c.ClientId);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(c => c.Nip)
            .HasMaxLength(10);

        builder.Property(c => c.AddressId)
               .IsRequired();

        builder.HasIndex(c => c.AddressId)
            .IsUnique();
    }
}

