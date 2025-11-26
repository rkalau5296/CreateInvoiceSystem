namespace CreateInvoiceSystem.Clients.Configuration;

using CreateInvoiceSystem.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {        
        builder.ToTable("Clients");
        builder.HasKey(c => c.ClientId);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(c => c.Nip)
            .HasMaxLength(10);

        builder.HasOne(c => c.Address)
            .WithMany()
            .HasForeignKey(c => c.AddressId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(c => c.AddressId)
            .IsUnique();
    }
}

