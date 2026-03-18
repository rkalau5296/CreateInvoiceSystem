using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;

namespace CreateInvoiceSystem.Modules.Users.Persistence.Configuration;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("AspNetUsers");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("UserId");

        builder.HasIndex(u => u.Nip).IsUnique();

        builder.Property(u => u.Name)
               .HasMaxLength(200);

        builder.Property(u => u.CompanyName)
               .HasMaxLength(200);

        builder.Property(u => u.Email)
               .HasMaxLength(256);

        builder.Property(u => u.Nip)
               .HasMaxLength(50);

        builder.Property(u => u.BankAccountNumber)
               .HasMaxLength(64);

        builder.Property(u => u.CreatedAt)
               .IsRequired();

        builder.Property(u => u.IsActive)
               .IsRequired();

        builder.Property(u => u.AddressId)
               .IsRequired();
    }
}